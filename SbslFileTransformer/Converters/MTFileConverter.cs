using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
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

        public static (string, string[]) RenameMTFile(string originalFile, ILogger logger)
        {
            try
            {
                lock (_locker)
                {
                    if (Path.GetFileName(originalFile).Split("_").Length > 2)
                        return (originalFile, new string[] { });

                    var lines = File.ReadAllLines(originalFile);

                    var pair = lines.FirstOrDefault(l => l.Trim().StartsWith(":28C:"))?.Split(":").Last();

                    if (pair != null)
                    {
                        var toRet = pair.Split("/");

                        var stmtSeq = pair.Replace("/", "");

                        if (Path.GetFileName(originalFile).Substring(6, stmtSeq.Length) != stmtSeq)
                        {
                            var newFilename = Path.Combine(Path.GetDirectoryName(originalFile), Path.GetFileName(originalFile).Insert(6, stmtSeq));

                            if (!File.Exists(newFilename))
                            {
                                File.Copy(originalFile, newFilename);
                            }
                            //File.Delete(originalFile);

                            return (newFilename, toRet);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error renaming file " + $"{originalFile}");
            }

            //_logger.LogInformation($"Skipping file {Path.GetFileName(originalFile)} because it does not have a sequence number");
            //send email maybe
            return (originalFile, new string[] { });
        }

        public static async Task RunMtSequenceValidationCheck(IServiceScopeFactory serviceScopeFactory, ILogger logger, EmailSender emailSender)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var uploadedToday = dbContext.UploadedFiles.Where(u => u.UploadedDate.Date == DateTime.Now.Date);

                    var dict = uploadedToday.GroupBy(u => u.MtStatementNo).Select(u => new { Stmt = u.Key, Max = u.Max(u => u.MtSequenceNo) }).ToList();

                    Dictionary<string, string> absent = new Dictionary<string, string>();

                    foreach (var stmt in dict)
                    {
                        var worked = int.TryParse(stmt.Max, out int result);

                        if (worked)
                        {
                            for (int i = 1; i <= Convert.ToInt32(result); i++)
                            {
                                if (uploadedToday.FirstOrDefault() == null)
                                {
                                    if (absent.ContainsKey(stmt.Stmt))
                                    {
                                        absent[stmt.Stmt] += i.ToString();
                                    }
                                    else
                                    {
                                        absent[stmt.Stmt] = i.ToString();
                                    }
                                }
                            }
                        }
                    }

                    if (absent.Where(a => !string.IsNullOrEmpty(a.Value)).Count() > 0)
                    {
                        StringBuilder message = new StringBuilder();

                        foreach (var val in absent)
                        {
                            message.AppendLine($"Statement No {val.Key} is missing Sequence Nos {val.Value}");
                        }

                        var config = await dbContext.Configurations.FirstOrDefaultAsync(c => c.ConfigType == ConfigurationType.Email && c.Key == "Recipients");

                        var recipients = config.Value.Split(new char[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        await emailSender.SendMessage(recipients, "Missing Seq. Nos", message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
    }
}
