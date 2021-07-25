using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static SbslFileTransformer.Converters.MTFileConverter;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class FileHelpers
    {
        private static readonly object _locker = new object();
        private static int delay = 0;

        public static void RestartService(string serviceName)
        {
            //https://stackoverflow.com/questions/28431621/how-to-restart-windows-service-by-itself-c-sharp
            // has to be 1 so that the service sees it as a failure and restarts itself

            Environment.Exit(1);
        }

        /// <summary>
        /// Main upload method
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="isProduction"></param>
        /// <param name="productionOrSandboxFolder"></param>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="logger"></param>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        public static async Task<bool> UploadFilesToSftp(IEnumerable<string> filePaths, bool isProduction, string productionOrSandboxFolder,
             IServiceScopeFactory serviceScopeFactory, ILogger logger, ConnectionInfo connectionInfo)
        {
            List<string> succeeded = new List<string>();

            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    await Task.Delay(delay);

                    client.Connect();

                    foreach (var filePath in filePaths)
                    {
                        lock (_locker)
                        {
                            MTFileValidation newFileName = ValidateMTFile(filePath, logger);

                            var previouslyUploaded = FileHasBeenUploadedBefore(filePath, isProduction, serviceScopeFactory);

                            if (previouslyUploaded.Uploaded)
                            {
                                continue;
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

                                var relativePath = Path.GetRelativePath(productionOrSandboxFolder, filePath);

                                remotePath = Path.Combine(remotePath, relativePath.Replace('\\', '/'));

                                if (sftpManager.UploadFile(filePath, remotePath, client))
                                {
                                    dbContext.UploadedFiles.Add(new SftpUploadedFile
                                    {
                                        FilePath = filePath,
                                        IsProduction = isProduction,
                                        Md5 = previouslyUploaded.Md5,
                                        Name = Path.GetFileName(filePath),
                                        Size = new FileInfo(filePath).Length,
                                        UploadedDate = DateTime.Now,
                                        MtAccountNo = newFileName.Account,
                                        MtStatementNo = newFileName.Statement,
                                        MtSequenceNo = string.Join(",", newFileName.Sequences)
                                    });

                                    dbContext.SaveChanges();

                                    succeeded.Add(filePath);

                                    logger.LogInformation($"Uploaded file to SFTP {remotePath} site successfully!");
                                }
                            }
                        }
                    }

                    client.Disconnect();

                    delay = 0;

                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error uploading file {ex.Message}");

                //THIS IS A CIRCUIT BREAKER CODE TO DELAY PASSWORD RETRY IF THERE IS A FAILURE
                DelayUploadAttemptDueToFailure();

                logger.LogWarning($"Delaying for {delay} ms");

                logger.LogWarning($"Failed to upload files {string.Join(',', filePaths.Except(succeeded))}");
            }

            return false;
        }

        private static void DelayUploadAttemptDueToFailure()
        {
            int toUse = 300000;//5 minutes incremental delay for use of wrong password

            if (delay > 0)
            {
                delay += toUse;
            }
            else
            {
                delay = toUse;
            }

            if (delay > 1800000) //30 minutes
            {
                delay = toUse;//reset back to 5 minutes
            }
        }

        public static UploadCheckResult FileHasBeenUploadedBefore(string filePath, bool isProduction,
            IServiceScopeFactory serviceScopeFactory)
        {
            lock (_locker)
            {
                var md5 = GetMd5(filePath);
                var name = Path.GetFileName(filePath);

                var uploadCheckResult = new UploadCheckResult
                {
                    Md5 = md5,
                    Uploaded = false
                };

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    //code to ignore duplicates for special scenarios goes here

                    //check if md5/filename exists
                    if (dbContext.UploadedFiles.Any(f =>
                        f.Md5.ToUpper() == md5.ToUpper() || f.Name.ToLower() == name.ToLower()))
                    {
                        uploadCheckResult.Uploaded = true;
                    }
                }

                return uploadCheckResult;
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

    public class UploadCheckResult
    {
        public bool Uploaded { get; set; }
        public string Md5 { get; set; }
    }
}