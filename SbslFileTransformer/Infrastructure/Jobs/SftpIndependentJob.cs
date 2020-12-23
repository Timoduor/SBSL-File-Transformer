using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Files;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Infrastructure.Plugins;
using SbslFileTransformer.Infrastructure.Sftp;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using SbslFileTransformer.PluginsLocal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private string Entity;

        private static readonly object _locker = new object();

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
                int prodTimeSpan, sbTimeSpan;

                GetConfiguration(out config, out prodTimeSpan, out sbTimeSpan);

                if (config.IncludeProduction)
                {
                    var fileWatcher = new InputFileWatcher(config.ProductionFolder, _fileLogger);

                    fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, true, config.ProductionFolder);

                    //sync all folders every hours
                    var timerProduction = new Timer((state) => RunFileCheckAndUpload(state, true, config.ProductionFolder).GetAwaiter().GetResult(), null, TimeSpan.Zero,
                                                            TimeSpan.FromMinutes(prodTimeSpan));
                }

                if (config.IncludeSandbox)
                {
                    var fileWatcher = new InputFileWatcher(config.SandboxFolder, _fileLogger);

                    fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, false, config.SandboxFolder);

                    var timerSandbox = new Timer((state) => RunFileCheckAndUpload(state, false, config.SandboxFolder).GetAwaiter().GetResult(), null, TimeSpan.Zero,
                                                    TimeSpan.FromMinutes(sbTimeSpan));

                }

                var timedValidator = new Timer((state) => MTFileConverter.RunMtSequenceValidationCheck(_serviceScopeFactory, _logger, _emailSender).GetAwaiter().GetResult(),
                                                null, TimeSpan.Zero, TimeSpan.FromMinutes(10)); //TODO


                _logger.LogInformation("Sftp Independent Job Started Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error starting SFTP independent job");
            }
        }

        private void GetConfiguration(out SftpConfigModel config, out int prodTimeSpan, out int sbTimeSpan)
        {
            prodTimeSpan = 15;
            sbTimeSpan = 5;
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
                Entity = configurations.FirstOrDefault(c => c.Key == "Entity")?.Value;
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
                        fileToProcess = await ProcessFileAndUpload(isProduction, productionOrSandboxFolder, file);
                    }
                }
                else
                {
                    fileToProcess = await ProcessFileAndUpload(isProduction, productionOrSandboxFolder, path);
                }

                _logger.LogInformation($"File check and upload ran successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + $" Error running file check and upload {fileToProcess}");
            }
        }

        private async Task<string> ProcessFileAndUpload(bool isProduction, string productionOrSandboxFolder, string file)
        {
            (string, string[]) newFileName = MTFileConverter.RenameMTFile(file, _logger);

            try
            {
                var uploadCheckResult = await StaticHelpers.FileHasBeenUploadedBefore(newFileName.Item1, isProduction, _serviceScopeFactory);

                if (uploadCheckResult.Item2)
                {
                    //_logger.LogInformation($"File {file} already uploaded!");
                    return string.Empty;
                }

                if (newFileName.Item2.Count() > 0)
                {
                    await StaticHelpers.UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1),
                        newFileName.Item2[0], newFileName.Item2.Count() == 1 ? string.Empty : newFileName.Item2[1], _serviceScopeFactory, _logger);
                }
                else
                {
                    bool isBalanceFile = false;

                    if (newFileName.Item1.ToLower().Contains("Nostro_Balances_Finacle_Format".ToLower()) && Path.GetExtension(newFileName.Item1.ToLower()) != ".txt")
                    {
                        isBalanceFile = true;

                        var converter = new BalanceFileConverter();

                        converter.Entity = Entity;

                        if (await converter.Execute(newFileName.Item1))
                        {
                            var newPath = Path.ChangeExtension(newFileName.Item1, ".txt");

                            await StaticHelpers.UploadFileToSftp(newPath, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newPath),
                                string.Empty, string.Empty, _serviceScopeFactory, _logger);
                        }
                    }

                    if (!isBalanceFile)
                    {
                        await StaticHelpers.UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction,
                            Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), string.Empty, string.Empty, _serviceScopeFactory, _logger);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error uploading file");
            }

            return newFileName.Item1;
        }



        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sftp Independent Job stopped");
        }
    }
}
