using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters
{
    public class MTFileConverter
    {
        private static object _locker = new object();

        static readonly SemaphoreSlim SemaphoreExtractor = new SemaphoreSlim(1,1);
        static readonly SemaphoreSlim SemaphoreValidator = new SemaphoreSlim(1, 1);

        public static (string, string, string[]) RenameMTFile(string originalFile, ILogger logger)
        {
            //if it is not in the statement folder
            if (!originalFile.ToLower().Contains("nostro") || !originalFile.ToLower().Contains("statement"))
                return (originalFile, string.Empty, new string[] { });

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

                        return (originalFile, account, toRet);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error renaming file " + $"{originalFile}");
            }

            return (originalFile, string.Empty, new string[] { });
        }

        public static async Task RunMtSequenceValidationCheck(IServiceScopeFactory serviceScopeFactory, ILogger logger, EmailSender emailSender)
        {
            try
            {
                await SemaphoreValidator.WaitAsync();

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var uploadedToday = dbContext.UploadedFiles.Where(u => u.UploadedDate.Date == DateTime.Now.Date && u.MtAccountNo != null);

                    var dict = uploadedToday.GroupBy(u => u.MtAccountNo).Select(u => new { Account = u.Key, Max = u.Max(u => u.MtSequenceNo) }).ToList();

                    //Dictionary<string, string> absent = new Dictionary<string, string>();

                    var absent = new List<MTFileValidation>();

                    foreach (var stmt in dict)
                    {
                        var StatementNos = uploadedToday.Where(u => u.MtAccountNo == stmt.Account).Select(u => u.MtStatementNo).Distinct().ToList();

                        foreach (var StatementNo in StatementNos)
                        {
                            var validation = new MTFileValidation { Account = stmt.Account, Statement = StatementNo, Sequences = new List<string>() };

                            var worked = int.TryParse(stmt.Max, out int result);

                            var present = uploadedToday.Where(u => u.MtAccountNo == stmt.Account && u.MtStatementNo == StatementNo).Select(u => u.MtSequenceNo).Distinct().ToList();

                            if (worked)
                            {
                                for (int i = 1; i <= result; i++)
                                {
                                    var current = i.ToString().PadLeft(5, '0');

                                    if (!present.Contains(current))
                                        validation.Sequences.Add(i.ToString());
                                }
                            }

                            absent.Add(validation);
                        }
                    }

                    StringBuilder message = new StringBuilder();

                    var unfinalized = uploadedToday.Where(u => !u.ProcessFor62F).Select(u => u.MtAccountNo).Distinct().ToList();

                    message.AppendLine();

                    foreach (var acc in unfinalized)
                    {
                        if (!uploadedToday.Where(u => u.ProcessFor62F && u.MtAccountNo == acc).Any())
                        {
                            message.AppendLine($"Account No. {acc} is missing Closing Balance Statement file");

                            message.AppendLine();
                        }
                    }

                    foreach (var val in absent)
                    {
                        var seqs = "";

                        foreach (var seq in val.Sequences)
                            seqs += seq + ", ";

                        message.AppendLine($"Account No. {val.Account} is missing Statement(s) {val.Statement} for Sequence Numbers: {seqs}");

                        message.AppendLine();
                    }

                    var config = await dbContext.Configurations.FirstOrDefaultAsync(c => c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

                    var recipients = config.Value.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    await emailSender.SendMessage(recipients, "Missing Closing Balances & Sequence Numbers", message.ToString());
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

        public static async Task RunMTBalanceExtractor(object location, bool isProduction, string sandboxOrProdFolder, IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            logger.LogInformation("Running MT Balance file extractor");

            try
            {
                await SemaphoreExtractor.WaitAsync();

                string loc = location.ToString();

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var entity = dbContext.Configurations.FirstOrDefault(f => f.Key == "Entity" && f.ConfigType == ConfigurationType.Setting).Value;

                    var encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                    var notProcessed = dbContext.UploadedFiles.Where(u => u.ProcessFor62F == false && u.FilePath.ToLower().Contains("statement"));

                    var paths = notProcessed.Select(f => f.FilePath).ToList();

                    if (paths.Count() > 0)
                    {
                        var options = new EnumerationOptions { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                        var filesInDirectory = Directory.GetFiles(loc, "*.*", options).ToList();

                        var filesInDirectoryToProcess = filesInDirectory.Where(f => paths.Any(p => f.ToLower() == p.ToLower())).ToList();

                        var resultFile = await ProcessFilesBalance(filesInDirectoryToProcess, sandboxOrProdFolder, entity, serviceScopeFactory);

                        if (File.Exists(resultFile))
                        {
                            var md5 = encryptionManager.GetMd5(resultFile);

                            if (FileHelpers.UploadFileToSftp(resultFile, md5, isProduction,
                                Path.GetFileName(resultFile), null, null, null,
                                serviceScopeFactory, logger))
                            {
                                foreach (var file in notProcessed)
                                {
                                    if (filesInDirectoryToProcess.Contains(file.FilePath,
                                        StringComparer.OrdinalIgnoreCase))
                                    {
                                        file.ProcessFor62F = true;
                                    }
                                }

                                dbContext.UpdateRange(notProcessed);

                                await dbContext.SaveChangesAsync();
                            }
                        }
                        logger.LogInformation($"Finished running balance file extractor on {paths.Count()} files");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                SemaphoreExtractor.Release();
            }
        }

        private static async Task<string> ProcessFilesBalance(List<string> filesToProcess, string sandboxOrProdFolder, string entity, IServiceScopeFactory serviceScopeFactory)
        {
            var outputPath = Path.Combine(sandboxOrProdFolder, $"MultiCurr_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            StringBuilder output = new StringBuilder();
            List<Balance> balances = new List<Balance>();

            //process mt files and return the balance files
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                foreach (var file in filesToProcess)
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
                    DateTime date = DateTime.ParseExact(balParts.Substring(1, 6), "yyMMdd", null);
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

                var maxValues = balances.Where(b => b.Date == balances.Where(d => d.Account == b.Account).Max(c => c.Date));

                foreach (var balance in balances)
                {
                    string toAppend = $"{entity}\t{await GetGLAccountNumber(balance.Account, dbContext)}\tNostros\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(balance.Date):MM/dd/yyyy}\t\t\t\t{balance.Amount}\t{balance.Currency}\n";

                    output.Append(toAppend);
                }

                if (!string.IsNullOrEmpty(output.ToString()))
                {
                    File.WriteAllText(outputPath, output.ToString());

                    //await Task.Delay(200);
                }
            }

            return outputPath;
        }

        private static async Task<string> GetGLAccountNumber(string accNo, ApplicationDbContext dbContext)
        {
            var account = (await dbContext.Accounts.FirstOrDefaultAsync(a => a.Account.ToLower() == accNo.ToLower() || a.Account.ToLower().Contains(accNo.ToLower())))?.Number;

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

        private class MTFileValidation
        {
            public string Account { get; set; }
            public string Statement { get; set; }
            public List<string> Sequences { get; set; }
        }
    }
}
