using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
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
        private string Entity;
        private List<Timer> _timers = new List<Timer>();
        bool _isRunning;

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting SFTP Independent...");

                SftpConfigModel config;
                int prodTimeSpan, sbTimeSpan;

                GetConfiguration(out config, out prodTimeSpan, out sbTimeSpan);

                if (config.IncludeProduction)
                {
                    //var fileWatcher = new InputFileWatcher(config.ProductionFolder, _fileLogger);

                    //fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, true, config.ProductionFolder);

                    //sync all folders every hours
                    var timerProduction = new Timer(async(state) => await RunFileCheckAndUpload(state, true, config.ProductionFolder), null, TimeSpan.Zero,
                                                            TimeSpan.FromMinutes(7));

                    _timers.Add(timerProduction);
                }

                if (config.IncludeSandbox)
                {
                    //var fileWatcher = new InputFileWatcher(config.SandboxFolder, _fileLogger);

                    //fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess, false, config.SandboxFolder);

                    var timerSandbox = new Timer(async(state) => await RunFileCheckAndUpload(state, false, config.SandboxFolder), null, TimeSpan.Zero,
                                                    TimeSpan.FromMinutes(sbTimeSpan));

                    _timers.Add(timerSandbox);
                }

                _logger.LogInformation("SFTP Independent Job Started Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error starting SFTP independent job");
            }

            return Task.CompletedTask;
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
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

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
            finally
            {
                _isRunning = false;
            }
        }

        private async Task<string> ProcessFileAndUpload(bool isProduction, string productionOrSandboxFolder, string file)
        {
            (string, string, string[]) newFileName = MTFileConverter.RenameMTFile(file, _logger);

            try
            {
                var uploadCheckResult = await FileHelpers.FileHasBeenUploadedBefore(newFileName.Item1, isProduction, _serviceScopeFactory);

                if (uploadCheckResult.Item2)
                {
                    //_logger.LogInformation($"File {file} already uploaded!");
                    return string.Empty;
                }

                //IF IT IS AN MT FILE
                if (newFileName.Item3.Count() > 0)
                {
                    await FileHelpers.UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction, Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), newFileName.Item2,
                        newFileName.Item3[0], newFileName.Item3.Count() == 1 ? string.Empty : newFileName.Item3[1], _serviceScopeFactory, _logger);
                }
                else
                {
                    await FileHelpers.UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction,
                        Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), string.Empty, string.Empty, string.Empty, _serviceScopeFactory, _logger);
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

            foreach (var timer in _timers)
            {
                timer?.Change(Timeout.Infinite, 0);
                await timer.DisposeAsync();
            }
        }

    }
}
