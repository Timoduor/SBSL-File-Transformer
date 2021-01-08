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
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters
{
    public class MTFileConverter
    {
        private static object _locker = new object();

        public static (string, string, string[]) RenameMTFile(string originalFile, ILogger logger)
        {
            try
            {
                lock (_locker)
                {
                    if (Path.GetFileName(originalFile).Split("_").Length > 2)
                        return (originalFile, string.Empty, new string[] { });

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

            //_logger.LogInformation($"Skipping file {Path.GetFileName(originalFile)} because it does not have a sequence number");
            //send email maybe
            return (originalFile, string.Empty, new string[] { });
        }

        public static async Task RunMtSequenceValidationCheck(IServiceScopeFactory serviceScopeFactory, ILogger logger, EmailSender emailSender)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var uploadedToday = dbContext.UploadedFiles.Where(u => u.UploadedDate.Date == DateTime.Now.Date && u.MtAccountNo != null);

                    var dict = uploadedToday.GroupBy(u => u.MtAccountNo).Select(u => new { Account = u.Key, Max = u.Max(u => u.MtSequenceNo) }).ToList();

                    Dictionary<string, string> absent = new Dictionary<string, string>();

                    foreach (var stmt in dict)
                    {
                        var worked = int.TryParse(stmt.Max, out int result);

                        var present = uploadedToday.Where(u => u.MtAccountNo == stmt.Account).Select(u => u.MtSequenceNo).ToList();

                        if (worked)
                        {
                            for (int i = 1; i <= Convert.ToInt32(result); i++)
                            {

                                var current = i.ToString().PadLeft(5, '0');

                                if (!present.Contains(current))
                                {
                                    if (absent.ContainsKey(stmt.Account))
                                    {
                                        absent[stmt.Account] += ", " + i.ToString();
                                    }
                                    else
                                    {
                                        absent[stmt.Account] = i.ToString();
                                    }
                                }
                            }
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

                    if (absent.Where(a => !string.IsNullOrEmpty(a.Value)).Count() > 0)
                    {

                        foreach (var val in absent)
                        {
                            message.AppendLine($"Account No. {val.Key} is missing Statement(s) for Sequence Numbers: {val.Value}");

                            message.AppendLine();
                        }

                        var config = await dbContext.Configurations.FirstOrDefaultAsync(c => c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

                        var recipients = config.Value.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        await emailSender.SendMessage(recipients, "Missing Closing Balances & Sequence Numbers", message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public static async Task RunMTBalanceExtractor(object location, bool isProduction, string sandboxOrProdFolder, IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            logger.LogInformation("Running MT Balance file extractor");

            try
            {
                string loc = location.ToString();

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var entity = dbContext.Configurations.FirstOrDefault(f => f.Key == "Entity" && f.ConfigType == ConfigurationType.Setting).Value;

                    var encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                    var notProcessed = dbContext.UploadedFiles.Where(u => u.ProcessFor62F == false);

                    var paths = notProcessed.Select(f => f.FilePath).ToList();

                    if (paths.Count() > 0)
                    {
                        var filesInDirectory = Directory.GetFiles(loc).ToList();

                        var filesInDirectoryToProcess = filesInDirectory.Where(f => paths.Any(p => f.ToLower() == p.ToLower())).ToList();

                        var resultFile = await ProcessFilesBalance(filesInDirectoryToProcess, sandboxOrProdFolder, entity, serviceScopeFactory);

                        if (File.Exists(resultFile))
                        {
                            var md5 = encryptionManager.GetMd5(resultFile);

                            if (await FileHelpers.UploadFileToSftp(resultFile, md5, isProduction, "", null, null, null, serviceScopeFactory, logger))
                            {
                                foreach (var file in notProcessed)
                                {
                                    if (filesInDirectoryToProcess.Contains(file.FilePath))
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

                    var account = lines.FirstOrDefault(l => l.Contains(":25:")).Split(":").Last().Trim();

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
            var account = (await dbContext.Accounts.FirstOrDefaultAsync(a => a.Account == accNo || a.Account.Contains(accNo)))?.Number;

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
    }
}
