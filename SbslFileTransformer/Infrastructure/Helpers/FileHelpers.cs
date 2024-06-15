using System;
using System.Collections.Generic;
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
using SbslFileTransformer.Infrastructure.Jobs;
using SbslFileTransformer.Infrastructure.Jobs.Others;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

using static SbslFileTransformer.Converters.MTFileConverter;

namespace SbslFileTransformer.Infrastructure.Helpers
{
    public static class FileHelpers
    {
        private static readonly object _locker = new object();
        private static int delay = 0;

        public static void RestartService()
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
            IEnumerable<string> filePathsToCheck = new List<string>();

            try
            {
                using (SftpClient client = new SftpClient(connectionInfo))
                {
                    await Task.Delay(delay);

                    client.Connect();

                    using (IServiceScope scope = serviceScopeFactory.CreateScope())
                    {
                        lock (_locker)
                        {
                            ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                            JobDisplayManager jobManager = scope.ServiceProvider.GetService<JobDisplayManager>();
                            string jobName = nameof(SftpIndependentJob);
                            JobStatus currentJobStatus = jobManager.GetJobStatus(jobName);

                            if (currentJobStatus == null)
                            {
                                currentJobStatus = new JobStatus(jobName) { Status = JobState.Running };

                                jobManager.SetJobStatus(jobName, currentJobStatus);
                            }

                            bool useUnicode = Convert.ToBoolean(dbContext.Configurations
                                            .FirstOrDefault(u => u.ConfigType == ConfigurationType.Sftp && u.Key == "UseUnicode")
                                            .Value);

                            List<SftpUploadedFile> uploadedFilesInDB = dbContext.UploadedFiles.ToList();

                            Dictionary<string, string> currentlyUploaded = uploadedFilesInDB.GroupBy(x => x.Md5).Select(f => f.FirstOrDefault())
                                .ToDictionary(f => f.Md5, f => f.Name);

                            filePathsToCheck = filePaths.Except(uploadedFilesInDB.Select(f => f.FilePath));

                            SftpManager sftpManager = scope.ServiceProvider.GetService<SftpManager>();
                            List<SftpUploadedFile> uploadedFiles = new List<SftpUploadedFile>();

                            UploadFilesToRemoteServer(isProduction, productionOrSandboxFolder, logger, succeeded, client, currentlyUploaded,
                                                            uploadedFiles, useUnicode, filePathsToCheck, sftpManager, jobManager, currentJobStatus);

                            dbContext.UploadedFiles.AddRange(uploadedFiles);
                            dbContext.SaveChanges();
                        }

                        client.Disconnect();

                        delay = 0;

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error uploading file {ex.Message}");

                //THIS IS A CIRCUIT BREAKER CODE TO DELAY PASSWORD RETRY IF THERE IS A FAILURE
                DelayUploadAttemptDueToFailure();

                logger.LogWarning($"Delaying for {delay} ms");

                logger.LogWarning($"Failed to upload files {string.Join(Environment.NewLine, filePathsToCheck.Except(succeeded))}");
            }

            return false;
        }

        private static void UploadFilesToRemoteServer(bool isProduction, string productionOrSandboxFolder, ILogger logger, List<string> succeeded,
            SftpClient client, Dictionary<string, string> currentlyUploaded, List<SftpUploadedFile> uploadedFiles, bool useUnicode,
            IEnumerable<string> filePathsToCheck, SftpManager sftpManager, JobDisplayManager jobManager, JobStatus currentJobStatus)
        {
            int count = 0;
            int total = filePathsToCheck.Count();

            filePathsToCheck = filePathsToCheck.OrderBy(f => new FileInfo(f).Length);//order by size to start with the smallest

            foreach (string filePath in filePathsToCheck)
            {
                try
                {
                    MTFileValidation newFileName = ValidateMTFile(filePath, logger);

                    UploadCheckResult previouslyUploaded = FileHasBeenUploadedBefore(filePath, currentlyUploaded);

                    count++;

                    currentJobStatus.SetProgress(count, total);
                    currentJobStatus.ProgressMessage = $"Currently uploading {filePath}... {count} of {total}";
                    jobManager.SetJobStatus(nameof(SftpIndependentJob), currentJobStatus);

                    if (previouslyUploaded.Uploaded)
                    {
                        continue;
                    }

                    if (uploadedFiles.Any(f => f.Md5 == previouslyUploaded.Md5))
                    {
                        continue;
                    }

                    logger.LogInformation($"Uploading file {filePath} to SFTP site at {DateTime.Now}!");

                    string remotePath = isProduction ? "/PROD/" : "/SB/";

                    //connecting to local cygwin SFTP server
                    if (useUnicode)
                    {
                        remotePath = "/cygdrive/e/Recon_Files/Files" + remotePath;

                        client.ConnectionInfo.Encoding = Encoding.Unicode;
                    }

                    string relativePath = Path.GetRelativePath(productionOrSandboxFolder, filePath);

                    remotePath = Path.Combine(remotePath, relativePath.Replace('\\', '/'));

                    if (sftpManager.UploadFile(filePath, remotePath, client))
                    {
                        uploadedFiles.Add(new SftpUploadedFile
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

                        succeeded.Add(filePath);

                        logger.LogInformation($"Uploaded file {filePath} to SFTP remote path {remotePath} site successfully!");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error uploading file {filePath} {ex.Message}");
                }
            }
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

        public static UploadCheckResult FileHasBeenUploadedBefore(string filePath, Dictionary<string, string> currentlyUploadedMd5Name)
        {
            lock (_locker)
            {
                string md5 = GetMd5(filePath);
                string name = Path.GetFileName(filePath);

                UploadCheckResult uploadCheckResult = new UploadCheckResult
                {
                    Md5 = md5,
                    Uploaded = false
                };

                //check if md5/filename exists
                if (currentlyUploadedMd5Name.Any(f =>
                    f.Key.ToUpper() == md5.ToUpper() || f.Value.ToLower() == name.ToLower()))
                {
                    uploadCheckResult.Uploaded = true;
                }


                return uploadCheckResult;
            }
        }

        public static string GetMd5(string filePath)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);

                    stream.Close();

                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static async Task<string> GetTempPath(IServiceScopeFactory serviceScopeFactory)
        {
            string backUpFolder = @"C:\SBSLETL_DbBackup";

            using (IServiceScope scope = serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                backUpFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                                   b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder"))
                               ?.Value ??
                               backUpFolder;
            }

            string tempFolderDirectory = Path.Combine(backUpFolder, "SBSLETL_Temp");

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
