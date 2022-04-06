using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Jobs.Extractors;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SbslFileTransformer.Infrastructure.Jobs.Others;

namespace SbslFileTransformer.Converters
{
    public class MTFileConverter
    {
        private static readonly object _locker = new object();

        private static readonly SemaphoreSlim SemaphoreExtractor = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim SemaphoreValidator = new SemaphoreSlim(1, 1);

        public static MTFileValidation ValidateMTFile(string originalFile, ILogger logger)
        {

            MTFileValidation validation = new MTFileValidation
            {
                Statement = Path.GetFileName(originalFile),
                Account = string.Empty,
                Sequences = new string[0].ToList(),
            };

            //if it is not in the statement folder
            if (!originalFile.ToLower().Contains("nostro") || !originalFile.ToLower().Contains("statement"))
                return validation;

            try
            {
                lock (_locker)
                {
                    string[] lines = File.ReadAllLines(originalFile);

                    string pair = lines.FirstOrDefault(l => l.Trim().StartsWith(":28C:"))?.Split(":").Last();
                    string account = lines.FirstOrDefault(l => l.Trim().StartsWith(":25:"))?.Split(":").Last();

                    if (pair != null)
                    {
                        string[] toRet = pair.Split("/");

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

                using (IServiceScope scope = serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Models.SftpUploadedFile> uploadedToday = dbContext.UploadedFiles.Where(u =>
                        u.UploadedDate.Date == DateTime.Now.Date && u.MtAccountNo != null).ToList();

                    var dict = uploadedToday.GroupBy(u => u.MtAccountNo)
                        .Select(u => new { Account = u.Key, Max = u.Max(u => u.MtSequenceNo) }).ToList();

                    //Dictionary<string, string> absent = new Dictionary<string, string>();

                    List<MTFileValidation> absent = new List<MTFileValidation>();

                    foreach (var stmt in dict)
                    {
                        List<string> StatementNos = uploadedToday.Where(u => u.MtAccountNo == stmt.Account)
                            .Select(u => u.MtStatementNo).Distinct().ToList();

                        foreach (string StatementNo in StatementNos)
                        {
                            MTFileValidation validation = new MTFileValidation
                            { Account = stmt.Account, Statement = StatementNo, Sequences = new List<string>() };

                            bool worked = int.TryParse(stmt.Max, out int result);

                            List<string> present = uploadedToday
                                .Where(u => u.MtAccountNo == stmt.Account && u.MtStatementNo == StatementNo)
                                .Select(u => u.MtSequenceNo).Distinct().ToList();

                            if (worked)
                                for (int i = 1; i <= result; i++)
                                {
                                    string current = i.ToString().PadLeft(5, '0');

                                    if (!present.Contains(current))
                                        validation.Sequences.Add(i.ToString());
                                }

                            absent.Add(validation);
                        }
                    }

                    StringBuilder message = new StringBuilder();

                    List<string> unfinalized = uploadedToday.Where(u => !u.ProcessFor62F).Select(u => u.MtAccountNo).Distinct()
                        .ToList();

                    message.AppendLine();

                    foreach (string acc in unfinalized)
                        if (!uploadedToday.Where(u => u.ProcessFor62F && u.MtAccountNo == acc).Any())
                        {
                            message.AppendLine($"Account No. {acc} is missing Closing Balance Statement file");

                            message.AppendLine();
                        }

                    foreach (MTFileValidation val in absent)
                    {
                        string seqs = "";

                        foreach (string seq in val.Sequences)
                            seqs += seq + ", ";

                        message.AppendLine(
                            $"Account No. {val.Account} is with Statement(s) {val.Statement} for is missing Sequence Numbers: {seqs}");

                        message.AppendLine();
                    }

                    List<Models.Configuration> configurations = await dbContext.Configurations.ToListAsync();

                    Models.Configuration config = configurations.FirstOrDefault(c =>
                        c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

                    string[] recipients = config.Value.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    if (!string.IsNullOrEmpty(message.ToString().Trim()))
                        await EmailHelpers.SendEmails(configurations, "Possible Missing Closing Balances & Sequence Numbers",
                            message.ToString(), null, emailSender);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                SemaphoreValidator.Release();
            }
        }

        public static async Task RunMTBalanceExtractor(object location, string sandboxOrProdFolder,
            IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            logger.LogInformation("Running MT Balance file extractor");
            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                EmailSender emailSender = scope.ServiceProvider.GetService<EmailSender>();
                JobDisplayManager jobManager = scope.ServiceProvider.GetService<JobDisplayManager>();
                string jobName = nameof(MtBalanceExtractorJob);

                List<Models.Configuration> configurations = await dbContext.Configurations.ToListAsync();

                JobStatus currentJobStatus = jobManager.GetJobStatus(jobName);

                if (currentJobStatus == null)
                {
                    currentJobStatus = new JobStatus(jobName) { Status = JobState.Starting };

                    jobManager.SetJobStatus(jobName, currentJobStatus);
                }

                List<string> filesInDirectoryToProcess = new List<string>();
                try
                {
                    await SemaphoreExtractor.WaitAsync();

                    string loc = location.ToString();

                    string entity = configurations
                        .FirstOrDefault(f => f.Key == "Entity" && f.ConfigType == ConfigurationType.Setting).Value;

                    EncryptionManager encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                    IQueryable<Models.SftpUploadedFile> notProcessed = dbContext.UploadedFiles.Where(u =>
                        u.ProcessFor62F == false && u.FilePath.ToLower().Contains("statement"));

                    List<string> pathsForNotProcessed = notProcessed.Select(f => f.FilePath).ToList();

                    SftpConfig sftpConfig = new SftpConfig
                    {
                        Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                        Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                        UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                        Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                        KeyFilesPath = configurations.FirstOrDefault(c => c.Key == "KeyFilesPath")?.Value
                    };

                    if (pathsForNotProcessed.Count() > 0)
                    {
                        EnumerationOptions options = new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            MatchCasing = MatchCasing.CaseInsensitive
                        };

                        List<string> filesInDirectory = Directory.GetFiles(loc, "*.*", options).ToList();

                        filesInDirectoryToProcess = filesInDirectory
                            .Where(f => pathsForNotProcessed.Any(p => f.ToLower() == p.ToLower()) && f.ToLower().Contains(entity.ToLower()))
                            .OrderBy(f => new FileInfo(f).LastWriteTime).ToList();

                        string multiCurrFile = await ProcessFilesBalance(filesInDirectoryToProcess, sandboxOrProdFolder,
                            entity, serviceScopeFactory);

                        if (File.Exists(multiCurrFile))
                        {
                            foreach (Models.SftpUploadedFile file in notProcessed.OrderBy(f => f.UploadedDate))
                                if (filesInDirectoryToProcess.Contains(file.FilePath,
                                    StringComparer.OrdinalIgnoreCase))
                                    file.ProcessFor62F = true;

                            dbContext.UpdateRange(notProcessed);

                            await dbContext.SaveChangesAsync();
                        }

                        logger.LogInformation($"Finished running balance file extractor on {pathsForNotProcessed.Count()} files");
                    }

                    currentJobStatus.Status = JobState.Completed;
                    jobManager.SetJobStatus(jobName, currentJobStatus);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);

                    await EmailHelpers.SendEmails(configurations, "Problem Converting CDM Balance files", $"\n\n {ex.Message}", filesInDirectoryToProcess, emailSender);
                }
                finally
                {
                    SemaphoreExtractor.Release();
                }
            }
        }

        private static async Task<string> ProcessFilesBalance(List<string> filesToProcess, string sandboxOrProdFolder,
            string entity, IServiceScopeFactory serviceScopeFactory)
        {
            string outputPath = Path.Combine(sandboxOrProdFolder,
                $"MultiCurr_{DateTime.Now:yyyyMMdd_HHmmss}_MT_{entity}.txt");

            StringBuilder output = new StringBuilder();
            List<Balance> balances = new List<Balance>();

            //process mt files and return the balance files
            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                ILogger<MTFileConverter> logger = scope.ServiceProvider.GetService<ILogger<MTFileConverter>>();
                JobDisplayManager jobManager = scope.ServiceProvider.GetService<JobDisplayManager>();
                string jobName = nameof(MtBalanceExtractorJob);

                JobStatus currentJobStatus = jobManager.GetJobStatus(jobName);

                if (currentJobStatus == null)
                {
                    currentJobStatus = new JobStatus(jobName) { Status = JobState.Starting };

                    jobManager.SetJobStatus(jobName, currentJobStatus);
                }

                currentJobStatus.Status = JobState.Running;
                jobManager.SetJobStatus(jobName, currentJobStatus);

                int count = 0;
                int total = filesToProcess.Count;

                foreach (string file in filesToProcess)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(file);

                        string bal = lines.FirstOrDefault(l => l.Contains(":62F:"));

                        if (bal == null) continue;

                        string account = lines.FirstOrDefault(l => l.Contains(":25:"))?.Split(":").Last().Trim();

                        string balParts = bal.Split(":").Last();

                        int sign = balParts[0] == 'C' ? 1 : -1;
                        DateTime date = DateTime.ParseExact(balParts.Substring(1, 6), "yyMMdd", null);
                        string currency = balParts.Substring(7, 3);
                        double amount = Convert.ToDouble(balParts.Substring(10).Replace(',', '.'));

                        Balance balance = new Balance
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

                IEnumerable<Balance> maxValues = balances.Where(b =>
                    b.Date == balances.Where(d => d.Account == b.Account).Max(c => c.Date));

                foreach (Balance balance in balances)
                {
                    string toAppend =
                        $"{entity}\t{await GetGLAccountNumber(balance.Account, dbContext)}\tNostros\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(balance.Date):MM/dd/yyyy}\t\t\t\t{balance.Amount}\t{balance.Currency}\n";

                    output.Append(toAppend);
                }

                if (!string.IsNullOrEmpty(output.ToString()))
                    File.WriteAllText(outputPath, output.ToString());
            }

            return outputPath;
        }

        private static async Task<string> GetGLAccountNumber(string accNo, ApplicationDbContext dbContext)
        {
            string account = (await dbContext.Accounts.FirstOrDefaultAsync(a =>
                a.Account.ToLower() == accNo.ToLower() || a.Account.ToLower().Contains(accNo.ToLower())))?.Number;

            if (!string.IsNullOrEmpty(account))
                return account;

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