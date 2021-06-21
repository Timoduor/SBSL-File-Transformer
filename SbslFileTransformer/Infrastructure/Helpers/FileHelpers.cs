using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class FileHelpers
    {
        private static readonly object _locker = new object();

        public static void RestartService(string serviceName)
        {
            //https://stackoverflow.com/questions/28431621/how-to-restart-windows-service-by-itself-c-sharp
            // has to be 1 so that the service sees it as a failure and restarts itself

            Environment.Exit(1);
        }

        public static bool UploadFileToSftp(string filePath, string md5, bool isProduction, string relativePath,
            string accountNo, string statementNo, string sequenceNo, IServiceScopeFactory serviceScopeFactory,
            ILogger logger, SftpClient client)
        {
            lock (_locker)
            {
                try
                {
                    var previouslyUploaded = FileHasBeenUploadedBefore(filePath, isProduction, serviceScopeFactory);

                    if (previouslyUploaded.Item2)
                    {
                        logger.LogWarning($"File {filePath} has been previously uploaded. Ignoring upload");
                        return true;
                    }

                    logger.LogInformation($"Uploading file {filePath} to SFTP site at {DateTime.Now}!");

                    using (var scope = serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        var useUnicode = Convert.ToBoolean(dbContext.Configurations
                            .FirstOrDefault(u => u.ConfigType == ConfigurationType.Sftp && u.Key == "UseUnicode")
                            .Value);

                        var sftpManager = scope.ServiceProvider.GetService<SftpManager>();

                        var remotePath = isProduction ? "/PROD/" : "/SB/";

                        //connecting to local cygwin SFTP server
                        if (useUnicode)
                        {
                            remotePath = "/cygdrive/e/Recon_Files/Files" + remotePath;

                            client.ConnectionInfo.Encoding = Encoding.Unicode;
                        }

                        remotePath = Path.Combine(remotePath, relativePath.Replace('\\', '/'));

                        if (sftpManager.UploadFile(filePath, remotePath, client))
                        {
                            dbContext.UploadedFiles.Add(new SftpUploadedFile
                            {
                                FilePath = filePath,
                                IsProduction = isProduction,
                                Md5 = md5,
                                Name = Path.GetFileName(filePath),
                                Size = new FileInfo(filePath).Length,
                                UploadedDate = DateTime.Now,
                                MtAccountNo = accountNo,
                                MtStatementNo = statementNo,
                                MtSequenceNo = sequenceNo
                            });

                            dbContext.SaveChanges();

                            logger.LogInformation($"Uploaded file to SFTP {remotePath} site successfully!");

                            return true;
                        }

                        logger.LogWarning($"Failed to upload file {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error uploading file {ex.Message}" + $"{filePath}");
                }

                return false;
            }
        }

        public static (string, bool) FileHasBeenUploadedBefore(string filePath, bool isProduction,
            IServiceScopeFactory serviceScopeFactory)
        {
            lock (_locker)
            {
                var md5 = GetMd5(filePath);
                var name = Path.GetFileName(filePath);

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    //check if md5/filename exists
                    if (dbContext.UploadedFiles.Any(f =>
                        f.Md5.ToUpper() == md5.ToUpper() || f.Name.ToLower() == name.ToLower())) return (md5, true);
                }

                return (md5, false);
            }
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

        public static async Task<string> GetTempPath(IServiceScopeFactory serviceScopeFactory)
        {
            var backUpFolder = @"C:\SBSLETL_DbBackup";

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                backUpFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                                   b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder"))
                               ?.Value ??
                               backUpFolder;
            }

            var tempFolderDirectory = Path.Combine(backUpFolder, "SBSLETL_Temp");

            if (!Directory.Exists(tempFolderDirectory))
                Directory.CreateDirectory(tempFolderDirectory);

            return tempFolderDirectory;
        }
    }
}