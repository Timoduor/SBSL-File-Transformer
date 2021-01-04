using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class StaticHelpers
    {
        public static void RestartService(string serviceName)
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/c net stop \"{serviceName}\" & net start \"{serviceName}\"";
            process.Start();
        }

        public static async Task<bool> UploadFileToSftp(string filePath, string md5, bool isProduction, string relativePath,
            string statementNo, string sequenceNo, IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            try
            {
                var previouslyUploaded = await FileHasBeenUploadedBefore(filePath, isProduction, serviceScopeFactory);

                if (previouslyUploaded.Item2)
                {
                    logger.LogWarning($"File {filePath} has been previously uploaded. Ignoring upload");
                    return true;
                }

                logger.LogInformation($"Uploading file {filePath} to SFTP site at {DateTime.Now}!");

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var sftpManager = scope.ServiceProvider.GetService<SftpManager>();

                    string remotePath = isProduction ? "/PROD/" : "/SB/";

                    remotePath = Path.Combine(remotePath, relativePath.Replace('\\', '/'));

                    if (sftpManager.UploadFile(filePath, remotePath))
                    {
                        dbContext.UploadedFiles.Add(new SftpUploadedFile
                        {
                            FilePath = filePath,
                            IsProduction = isProduction,
                            Md5 = md5,
                            Name = Path.GetFileName(filePath),
                            Size = new FileInfo(filePath).Length,
                            UploadedDate = DateTime.Now,
                            MtStatementNo = statementNo,
                            MtSequenceNo = sequenceNo
                        });

                        await dbContext.SaveChangesAsync();

                        logger.LogInformation($"Uploaded file to SFTP {remotePath} site successfully!");

                        return true;
                    }
                    else
                    {
                        logger.LogWarning($"Failed to upload file {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error uploading file {ex.Message}" + $"{filePath}");
            }

            return false;
        }

        public static async Task<(string, bool)> FileHasBeenUploadedBefore(string filePath, bool isProduction, IServiceScopeFactory serviceScopeFactory)
        {
            string md5 = string.Empty;

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                md5 = encryptionManager.GetMd5(filePath);

                var fileName = Path.GetFileName(filePath);

                //check if md5/filename exists
                if (await dbContext.UploadedFiles.AnyAsync(f => (f.Md5.ToUpper() == md5.ToUpper() && f.IsProduction == isProduction)
                                    || (f.Name.ToUpper() == fileName.ToUpper() && f.IsProduction == isProduction)))
                {
                    return (md5, true);
                }
            }
            return (md5, false);
        }

        public static List<AccountsLookup> GetAccountFromCsv(string file)
        {
            var list = new List<AccountsLookup>();

            file = string.IsNullOrEmpty(file) ? @"C:\Users\Yida\Downloads\GL_BANK_LOOKUP.csv" : file;

            using (var reader = new StreamReader(file))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<AccountsLookup>();

                    list.AddRange(records.ToList());
                }
            }

            return list;
        }

        public static string GetMd5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);

                    stream.Close();

                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static DateTime GetLastDayOfTheMonth(DateTime date2)
        {
            return new DateTime(date2.Year, date2.Month, 1).AddMonths(1).AddDays(-1);
        }

        public static DateTime GetLastBusinessDayOfMonth(DateTime date)
        {
            //exclude holidays https://stackoverflow.com/questions/273048/how-to-determine-the-last-business-day-in-a-given-month
            var holidays = new List<DateTime> {/* list of observed holidays */};
            DateTime lastBusinessDay = new DateTime();
            var i = DateTime.DaysInMonth(date.Year, date.Month);
            while (i > 0)
            {
                var dtCurrent = new DateTime(date.Year, date.Month, i);
                if (dtCurrent.DayOfWeek < DayOfWeek.Saturday && dtCurrent.DayOfWeek > DayOfWeek.Sunday)
                {
                    lastBusinessDay = dtCurrent;
                    i = 0;
                }
                else
                {
                    i = i - 1;
                }
            }

            return lastBusinessDay;
        }

    }


}
