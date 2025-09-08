using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Jobs.Extractors;
using SbslFileTransformer.Infrastructure.Jobs.Others;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Converters
{
    public class MTFileConverter
    {
        private static readonly object _locker = new object();

        private static readonly SemaphoreSlim SemaphoreExtractor = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim SemaphoreValidator = new SemaphoreSlim(1, 1);

        public static MTFileValidation ValidateMTFile(string originalFile, ILogger logger)
        {

            var validation = new MTFileValidation
            {
                Statement = Path.GetFileName(originalFile),
                Account = string.Empty,
                Sequences = new string[0].ToList(),
            };

            //if it is not in the statement folder
            if (!originalFile.ToLower().Contains("nostro") || !originalFile.ToLower().Contains("statement"))
            {
                return validation;
            }

            try
            {
                lock (_locker)
                {
                    var lines = File.ReadAllLines(originalFile);

                    var pair = lines.FirstOrDefault(l => l.Trim().StartsWith(":28C:"))?.Split(":").Last();
                    var account = lines.FirstOrDefault(l => l.Trim().StartsWith(":25:"))?.Split(":").Last();

                    if (pair != null)
                    {
                        var toRet = pair.Split("/");

                        validation.Statement = Path.GetFileName(originalFile);
                        validation.Account = account;
                        validation.Sequences = toRet.ToList();

                        return validation;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error renaming file " + $"{originalFile}");
            }

            return validation;
        }

        public static async Task RunMtSequenceValidationCheck(IServiceScopeFactory serviceScopeFactory, ILogger logger,
            EmailSender emailSender)
        {
            try
            {
                await SemaphoreValidator.WaitAsync();

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var uploadedToday = dbContext.UploadedFiles.Where(u =>
                        u.UploadedDate.Date == DateTime.Now.Date && u.MtAccountNo != null).ToList();

                    var dict = uploadedToday.GroupBy(u => u.MtAccountNo)
                        .Select(u => new { Account = u.Key, Max = u.Max(u => u.MtSequenceNo) }).ToList();

                    //Dictionary<string, string> absent = new Dictionary<string, string>();

                    var absent = new List<MTFileValidation>();

                    foreach (var stmt in dict)
                    {
                        var StatementNos = uploadedToday.Where(u => u.MtAccountNo == stmt.Account)
                            .Select(u => u.MtStatementNo).Distinct().ToList();

                        foreach (var StatementNo in StatementNos)
                        {
                            var validation = new MTFileValidation
                            { Account = stmt.Account, Statement = StatementNo, Sequences = new List<string>() };

                            var worked = int.TryParse(stmt.Max, out var result);

                            var present = uploadedToday
                                .Where(u => u.MtAccountNo == stmt.Account && u.MtStatementNo == StatementNo)
                                .Select(u => u.MtSequenceNo).Distinct().ToList();

                            if (worked)
                            {
                                for (var i = 1; i <= result; i++)
                                {
                                    var current = i.ToString().PadLeft(5, '0');

                                    if (!present.Contains(current))
                                    {
                                        validation.Sequences.Add(i.ToString());
                                    }
                                }
                            }

                            absent.Add(validation);
                        }
                    }

                    var message = new StringBuilder();

                    var unfinalized = uploadedToday.Where(u => !u.ProcessFor62F).Select(u => u.MtAccountNo).Distinct()
                        .ToList();

                    _ = message.AppendLine();

                    foreach (var acc in unfinalized)
                    {
                        if (!uploadedToday.Where(u => u.ProcessFor62F && u.MtAccountNo == acc).Any())
                        {
                            _ = message.AppendLine($"Account No. {acc} is missing Closing Balance Statement file");

                            _ = message.AppendLine();
                        }
                    }

                    foreach (var val in absent)
                    {
                        var seqs = "";

                        foreach (var seq in val.Sequences)
                        {
                            seqs += seq + ", ";
                        }

                        _ = message.AppendLine(
                            $"Account No. {val.Account} is with Statement(s) {val.Statement} for is missing Sequence Numbers: {seqs}");

                        _ = message.AppendLine();
                    }

                    var configurations = await dbContext.Configurations.ToListAsync();

                    var config = configurations.FirstOrDefault(c =>
                        c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

                    var recipients = config.Value.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    if (!string.IsNullOrEmpty(message.ToString().Trim()))
                    {
                        await EmailHelpers.SendEmails(configurations, "Possible Missing Closing Balances & Sequence Numbers",
                            message.ToString(), null, emailSender, logger);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                _ = SemaphoreValidator.Release();
            }
        }

        public static async Task RunMTBalanceExtractor(object location, string sandboxOrProdFolder,
            IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            logger.LogInformation("Running MT Balance file extractor");
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var emailSender = scope.ServiceProvider.GetService<EmailSender>();
                var jobManager = scope.ServiceProvider.GetService<JobDisplayManager>();
                var jobName = nameof(MtBalanceExtractorJob);

                var configurations = await dbContext.Configurations.ToListAsync();

                var currentJobStatus = jobManager.GetJobStatus(jobName);

                if (currentJobStatus == null)
                {
                    currentJobStatus = new JobStatus(jobName) { Status = JobState.Starting };

                    jobManager.SetJobStatus(jobName, currentJobStatus);
                }

                var filesInDirectoryToProcess = new List<string>();
                try
                {
                    await SemaphoreExtractor.WaitAsync();

                    var loc = location.ToString();

                    var entity = configurations
                        .FirstOrDefault(f => f.Key == "Entity" && f.ConfigType == ConfigurationType.Setting).Value;

                    var encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                    var notProcessed = dbContext.UploadedFiles.Where(u =>
                        u.ProcessFor62F == false && u.FilePath.ToLower().Contains("statement"));

                    var pathsForNotProcessed = notProcessed.Select(f => f.FilePath).ToList();

                    var sftpConfig = new SftpConfig
                    {
                        Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                        Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                        UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                        Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                        KeyFilesPath = configurations.FirstOrDefault(c => c.Key == "KeyFilesPath")?.Value
                    };

                    if (pathsForNotProcessed.Count() > 0)
                    {
                        var options = new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            MatchCasing = MatchCasing.CaseInsensitive
                        };

                        var filesInDirectory = Directory.GetFiles(loc, "*.*", options).ToList();

                        filesInDirectoryToProcess = filesInDirectory
                            .Where(f => pathsForNotProcessed.Any(p => f.ToLower() == p.ToLower()) && f.ToLower().Contains(entity.ToLower()))
                            .OrderBy(f => new FileInfo(f).LastWriteTime).ToList();

                        var multiCurrFile = await ProcessFilesBalance(filesInDirectoryToProcess, sandboxOrProdFolder,
                            entity, serviceScopeFactory);

                        if (File.Exists(multiCurrFile))
                        {
                            foreach (var file in notProcessed.OrderBy(f => f.UploadedDate))
                            {
                                if (filesInDirectoryToProcess.Contains(file.FilePath,
                                    StringComparer.OrdinalIgnoreCase))
                                {
                                    file.ProcessFor62F = true;
                                }
                            }

                            dbContext.UpdateRange(notProcessed);

                            _ = await dbContext.SaveChangesAsync();
                        }

                        logger.LogInformation($"Finished running balance file extractor on {pathsForNotProcessed.Count()} files");
                    }

                    currentJobStatus.Status = JobState.Completed;
                    jobManager.SetJobStatus(jobName, currentJobStatus);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    await EmailHelpers.SendEmails(configurations, "Problem Converting CDM Balance files", $"\n\n {ex.Message}", filesInDirectoryToProcess, emailSender, logger);
                }
                finally
                {
                    _ = SemaphoreExtractor.Release();
                }
            }
        }

        private static async Task<string> ProcessFilesBalance(List<string> filesToProcess, string sandboxOrProdFolder,
            string entity, IServiceScopeFactory serviceScopeFactory)
        {
            var outputPath = Path.Combine(sandboxOrProdFolder,
                $"MultiCurr_{DateTime.Now:yyyyMMdd_HHmmss}_MT_{entity}.txt");

            var output = new StringBuilder();
            var balances = new List<Balance>();

            //process mt files and return the balance files
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetService<ILogger<MTFileConverter>>();
                var jobManager = scope.ServiceProvider.GetService<JobDisplayManager>();
                var jobName = nameof(MtBalanceExtractorJob);

                var currentJobStatus = jobManager.GetJobStatus(jobName);

                if (currentJobStatus == null)
                {
                    currentJobStatus = new JobStatus(jobName) { Status = JobState.Starting };

                    jobManager.SetJobStatus(jobName, currentJobStatus);
                }

                currentJobStatus.Status = JobState.Running;
                jobManager.SetJobStatus(jobName, currentJobStatus);

                var count = 0;
                var total = filesToProcess.Count;

                foreach (var file in filesToProcess)
                {
                    try
                    {
                        var lines = File.ReadAllLines(file);

                        var bal = lines.FirstOrDefault(l => l.Contains(":62F:"));

                        if (bal == null)
                        {
                            continue;
                        }

                        var account = lines.FirstOrDefault(l => l.Contains(":25:"))?.Split(":").Last().Trim();

                        var balParts = bal.Split(":").Last();

                        var sign = balParts[0] == 'C' ? 1 : -1;
                        var date = DateTime.ParseExact(balParts.Substring(1, 6), "yyMMdd", null);
                        var currency = balParts.Substring(7, 3);
                        var amount = Convert.ToDouble(balParts.Substring(10).Replace(',', '.'));

                        var balance = new Balance
                        {
                            Account = account,
                            Date = date,
                            Currency = currency,
                            Entity = entity,
                            Amount = amount * sign
                        };

                        balances.Add(balance);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }

                    currentJobStatus.ProgressMessage = $"Currently processing {file}... {count} of {total}";
                    currentJobStatus.SetProgress(count, total);
                    jobManager.SetJobStatus(jobName, currentJobStatus);
                }

                var maxValues = balances.Where(b =>
                    b.Date == balances.Where(d => d.Account == b.Account).Max(c => c.Date));

                foreach (var balance in balances)
                {
                    var toAppend =
                        $"{entity}\t{await GetGLAccountNumber(balance.Account, dbContext, logger)}\tNostros\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(balance.Date):MM/dd/yyyy}\t\t\t\t{balance.Amount}\t{balance.Currency}\n";

                    _ = output.Append(toAppend);
                }

                if (!string.IsNullOrEmpty(output.ToString()))
                {
                    File.WriteAllText(outputPath, output.ToString());
                }
            }

            return outputPath;
        }

        private static async Task<string> GetGLAccountNumber(string accNo, ApplicationDbContext dbContext, ILogger<MTFileConverter> logger)
        {
            if (string.IsNullOrEmpty(accNo))
            {
                logger.LogError($"Missing accNo {accNo} Returning empty string!");
                return string.Empty;
            }

            var account = (await dbContext.Accounts.FirstOrDefaultAsync(a =>
                a.Account.ToLower() == accNo.ToLower() || a.Account.ToLower().Contains(accNo.ToLower())))?.Number;

            if (!string.IsNullOrEmpty(account))
            {
                return account;
            }

            return accNo;
        }

        private class Balance
        {
            public string Entity { get; set; }
            public string Account { get; set; }
            public DateTime Date { get; set; }
            public double Amount { get; set; }
            public string Currency { get; set; }
        }

        public class MTFileValidation
        {
            //original file name
            public string Account { get; set; }
            //
            public string Statement { get; set; }
            public List<string> Sequences { get; set; }
        }
    }
}
