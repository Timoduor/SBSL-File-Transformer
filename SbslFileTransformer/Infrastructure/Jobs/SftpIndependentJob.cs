using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Files;
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

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger
            , ILogger<InputFileWatcher> fileLogger, EncryptionManager encryptionManager)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _fileLogger = fileLogger;
            _encryptionManager = encryptionManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
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
                        Password = _encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value),
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }


        private async Task RunFileCheckAndUpload(object state, bool isProduction, string productionOrSandboxFolder)
        {
            try
            {
                var path = state?.ToString();

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !File.Exists(path))
                {
                    //do check for all folders/files
                    var options = new EnumerationOptions {
                        MatchCasing = MatchCasing.CaseInsensitive,
                        MatchType = MatchType.Simple,
                        RecurseSubdirectories = true
                    };

                    var files = Directory.GetFiles(productionOrSandboxFolder, "*.*", options);

                    foreach (var file in files)
                    {
                        var uploadResult = await FileHasBeenUploadedBefore(file, isProduction);

                        await UploadFileToSftp(file, uploadResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, file));
                    }
                }
                else
                {
                    //do check for specific file
                    var uploadResult = await FileHasBeenUploadedBefore(path, isProduction);

                    await UploadFileToSftp(path, uploadResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, path));
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task UploadFileToSftp(string filePath, string md5, bool isProduction, string relativePath)
        {
            try
            {
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
                            UploadedDate = DateTime.Now
                        });

                        await dbContext.SaveChangesAsync();

                        _logger.LogInformation($"File {filePath} has been successfully uploaded");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to upload file {filePath}");
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task<(string, bool)> FileHasBeenUploadedBefore(string filePath, bool isProduction)
        {
            var md5 = _encryptionManager.GetMd5(filePath);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                if (await dbContext.UploadedFiles.AnyAsync(f => f.Md5 == md5 && f.IsProduction == isProduction))
                {
                    return (md5, true);
                }
            }
            return (md5, false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
