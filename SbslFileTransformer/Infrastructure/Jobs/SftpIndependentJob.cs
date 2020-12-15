using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Files;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class SftpIndependentJob : IHostedService
    {
        readonly IServiceScopeFactory _serviceScopeFactory;
        readonly ILogger<SftpIndependentJob> _logger;
        private ILogger<InputFileWatcher> _fileLogger;
        private readonly EncryptionManager _encryptionManager;
        private readonly EmailSender _emailSender;

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger
            , ILogger<InputFileWatcher> fileLogger, EncryptionManager encryptionManager, EmailSender emailSender)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _fileLogger = fileLogger;
            _encryptionManager = encryptionManager;
            _emailSender = emailSender;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting Sftp Independent...");

                SftpConfigModel config;

                int prodTimeSpan = 15;
                int sbTimeSpan = 5;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

                    config = new SftpConfigModel
                    {
                        Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                        Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                        UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                        //Password = _encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value),
                        RecurseFolders = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "RecurseFolders")?.Value),
                        IncludeSandbox = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeSandbox")?.Value),
                        IncludeProduction = Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value),
                        ProductionFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value,
                        SandboxFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value,
                    };

                    prodTimeSpan = Convert.ToInt32(dbContext.Configurations.FirstOrDefault(c => c.Key == "ProductionTimeSpanCheck")?.Value);
                    sbTimeSpan = Convert.ToInt32(dbContext.Configurations.FirstOrDefault(c => c.Key == "SandboxTimeSpanCheck")?.Value);
                }

                if (config.IncludeProduction)
                {
                    var fileWatcher = new InputFileWatcher(config.ProductionFolder, _fileLogger);

                    fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, true, config.ProductionFolder);

                    //sync all folders every hours
                    var timerProduction = new Timer(async (state) => await RunFileCheckAndUpload(state, true, config.ProductionFolder), null, TimeSpan.Zero,
                    TimeSpan.FromMinutes(prodTimeSpan));
                }

                if (config.IncludeSandbox)
                {
                    var fileWatcher = new InputFileWatcher(config.SandboxFolder, _fileLogger);

                    fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, false, config.SandboxFolder);

                    var timerSandbox = new Timer(async (state) => await RunFileCheckAndUpload(state, false, config.SandboxFolder), null, TimeSpan.Zero,
                                                    TimeSpan.FromMinutes(sbTimeSpan));

                }

                _logger.LogInformation("Sftp Independent Job Started Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }


        private async Task RunFileCheckAndUpload(object state, bool isProduction, string productionOrSandboxFolder)
        {

            string fileToProcess = string.Empty;

            try
            {
                _logger.LogInformation($"Running file check and upload at {DateTime.Now}!");

                var path = state?.ToString();

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !File.Exists(path))
                {
                    //do check for all folders/files
                    var options = new EnumerationOptions
                    {
                        MatchCasing = MatchCasing.CaseInsensitive,
                        MatchType = MatchType.Simple,
                        RecurseSubdirectories = true
                    };

                    var files = Directory.GetFiles(productionOrSandboxFolder, "*.*", options);

                    foreach (var file in files)
                    {
                        var newFileName = RenameMTFile(file);

                        var uploadCheckResult = await FileHasBeenUploadedBefore(newFileName.Item1, isProduction);

                        if (newFileName.Item2.Count() > 0)
                        {
                            await UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), newFileName.Item2[0], newFileName.Item2[1]);
                        }
                        else
                        {
                            await UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), string.Empty, string.Empty);
                        }

                        fileToProcess = newFileName.Item1;
                    }
                }
                else
                {
                    var newFileName = RenameMTFile(path);

                    //do check for specific file
                    var uploadResult = await FileHasBeenUploadedBefore(newFileName.Item1, isProduction);

                    if (newFileName.Item2.Count() > 0)
                    {

                        await UploadFileToSftp(newFileName.Item1, uploadResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), newFileName.Item2[0], newFileName.Item2[1]);
                    }
                    else
                    {
                        await UploadFileToSftp(newFileName.Item1, uploadResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), string.Empty, string.Empty);
                    }

                    fileToProcess = newFileName.Item1;
                }

                _logger.LogInformation($"File check and upload ran successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + $" {fileToProcess}");
            }
        }

        private (string, string[]) RenameMTFile(string originalFile)
        {
            try
            {
                var lines = File.ReadAllLines(originalFile);

                var pair = lines.FirstOrDefault(l => l.Trim().StartsWith(":28C:"))?.Split(":").Last();

                if (pair != null)
                {
                    var toRet = pair.Split("/");

                    var stmtSeq = pair.Replace("/", "_");

                    _logger.LogInformation($"Skipping file {Path.GetFileName(originalFile)} because it does not have a sequence number");

                    //send email maybe

                    var newFilename = Path.Combine(Path.GetDirectoryName(originalFile), stmtSeq + "_" + Path.GetFileName(originalFile));

                    File.Move(originalFile, newFilename);

                    return (newFilename, toRet);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message + $"{originalFile}");
            }

            return (originalFile, new string[] { });
        }

        private async Task UploadFileToSftp(string filePath, string md5, bool isProduction, string relativePath, string statementNo, string sequenceNo)
        {
            try
            {
                _logger.LogInformation($"Uploading file {filePath} to SFTP site at {DateTime.Now}!");

                var previouslyUploaded = await FileHasBeenUploadedBefore(filePath, isProduction);

                if (previouslyUploaded.Item2)
                {
                    _logger.LogWarning($"File {filePath} has been previously uploaded. Ignoring upload");
                    return;
                }

                using (var scope = _serviceScopeFactory.CreateScope())
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

                        _logger.LogInformation($"Uploaded file to SFTP {remotePath} site successfully!");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to upload file {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + $"{filePath}");
            }
        }

        private async Task<(string, bool)> FileHasBeenUploadedBefore(string filePath, bool isProduction)
        {
            var md5 = _encryptionManager.GetMd5(filePath);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var fileName = Path.GetFileName(filePath);

                //check if md5/filename exists
                if (await dbContext.UploadedFiles.AnyAsync(f => (f.Md5 == md5 && f.IsProduction == isProduction)
                                    || (f.Name == fileName && f.IsProduction == isProduction)))
                {
                    return (md5, true);
                }
            }
            return (md5, false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sftp Independent Job stopped");
        }
    }
}
