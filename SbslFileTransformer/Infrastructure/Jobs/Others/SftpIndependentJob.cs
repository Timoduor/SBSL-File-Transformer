using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class SftpIndependentJob : IHostedService
    {
        private static SemaphoreSlim _semaphore;
        private readonly ILogger<SftpIndependentJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private EncryptionManager _encryptionManager;
        private readonly List<Timer> _timers = new List<Timer>();
        private string Entity;

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger,
            EncryptionManager encryptionManager)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _encryptionManager = encryptionManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting SFTP Independent...");

                _semaphore = new SemaphoreSlim(1, 1);

                SftpConfigModel config;
                int prodTimeSpan, sbTimeSpan;

                GetConfiguration(out config, out prodTimeSpan, out sbTimeSpan);

                if (config.IncludeProduction)
                {
                    var timerProd = new Timer(
                        async state => await RunFileCheckAndUpload(state, true, config.ProductionFolder, config), null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(prodTimeSpan));

                    _timers.Add(timerProd);
                }

                if (config.IncludeSandbox)
                {
                    var timerSB = new Timer(
                        async state => await RunFileCheckAndUpload(state, false, config.SandboxFolder, config), null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(sbTimeSpan));

                    _timers.Add(timerSB);
                }

                _logger.LogInformation("SFTP Independent Job Started Successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error starting SFTP independent job");
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SFTP Independent Job stopped");

            foreach (var timer in _timers) await timer.DisposeAsync();
        }

        private void GetConfiguration(out SftpConfigModel config, out int prodTimeSpan, out int sbTimeSpan)
        {
            prodTimeSpan = 15;
            sbTimeSpan = 5;
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp)
                    .ToList();

                config = new SftpConfigModel
                {
                    Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                    Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                    UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                    Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                    RecurseFolders =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "RecurseFolders")?.Value),
                    IncludeSandbox =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeSandbox")?.Value),
                    IncludeProduction =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value),
                    ProductionFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value,
                    SandboxFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value,
                    KeyFilesPath = configurations.FirstOrDefault(c => c.Key == "KeyFilesPath")?.Value,
                    UseUnicode =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "UseUnicode")?.Value ?? "False")
                };

                prodTimeSpan = Convert.ToInt32(dbContext.Configurations
                    .FirstOrDefault(c => c.Key == "ProductionTimeSpanCheck")?.Value);
                sbTimeSpan = Convert.ToInt32(dbContext.Configurations
                    .FirstOrDefault(c => c.Key == "SandboxTimeSpanCheck")?.Value);
                Entity = configurations.FirstOrDefault(c => c.Key == "Entity")?.Value;
            }
        }

        private async Task RunFileCheckAndUpload(object state, bool isProduction, string productionOrSandboxFolder,
            SftpConfigModel config)
        {
            var fileToProcess = string.Empty;

            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation($"Running file check and upload at {DateTime.Now}!");

                var path = state?.ToString();

                ConnectionInfo connectionInfo = string.IsNullOrEmpty(config.Password?.Trim())
                    ? connectionInfo = new ConnectionInfo(config.Host, config.Port, config.UserName,
                        new NoneAuthenticationMethod(config.UserName))
                    : new ConnectionInfo(config.Host, config.Port, config.UserName,
                        new PasswordAuthenticationMethod(config.UserName, config.Password));

                if (!string.IsNullOrEmpty(config.KeyFilesPath?.Trim()))
                {
                    var keyFiles = Directory.GetFiles(config.KeyFilesPath).Select(f => new PrivateKeyFile(f)).ToArray();

                    connectionInfo = new ConnectionInfo(config.Host, config.Port, config.UserName,
                        new PrivateKeyAuthenticationMethod(config.UserName, keyFiles));
                }

                if (config.UseUnicode) connectionInfo.Encoding = Encoding.UTF8;

                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !File.Exists(path))
                    {
                        //do check for all folders/files
                        var options = new EnumerationOptions
                        {
                            MatchCasing = MatchCasing.CaseInsensitive,
                            MatchType = MatchType.Simple,
                            RecurseSubdirectories = true
                        };

                        var files = Directory.GetFiles(productionOrSandboxFolder, "*", options);

                        foreach (var file in files)
                            fileToProcess = ProcessFileAndUpload(isProduction, productionOrSandboxFolder, file, client);
                    }
                    else
                    {
                        fileToProcess = ProcessFileAndUpload(isProduction, productionOrSandboxFolder, path, client);
                    }

                    client.Disconnect();
                }

                _logger.LogInformation("File check and upload ran successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + $" Error running file check and upload {fileToProcess}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string ProcessFileAndUpload(bool isProduction, string productionOrSandboxFolder, string file,
            SftpClient client)
        {
            var newFileName = MTFileConverter.RenameMTFile(file, _logger);

            try
            {
                var uploadCheckResult =
                    FileHelpers.FileHasBeenUploadedBefore(newFileName.Item1, isProduction, _serviceScopeFactory);

                if (uploadCheckResult.Item2)
                    //_logger.LogInformation($"File {file} already uploaded!");
                    return string.Empty;

                //IF IT IS AN MT FILE
                if (newFileName.Item3.Count() > 0)
                    FileHelpers.UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction,
                        Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), newFileName.Item2,
                        newFileName.Item3[0], newFileName.Item3.Count() == 1 ? string.Empty : newFileName.Item3[1],
                        _serviceScopeFactory, _logger, client);
                else
                    FileHelpers.UploadFileToSftp(newFileName.Item1, uploadCheckResult.Item1, isProduction,
                        Path.GetRelativePath(productionOrSandboxFolder, newFileName.Item1), string.Empty, string.Empty,
                        string.Empty, _serviceScopeFactory, _logger, client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error uploading file");
            }

            return newFileName.Item1;
        }
    }

    public class SftpConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string KeyFilesPath { get; set; }
    }
}