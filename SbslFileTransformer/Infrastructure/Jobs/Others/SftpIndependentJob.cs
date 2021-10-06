using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
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
        private static SemaphoreSlim _semaphore;
        private readonly ILogger<SftpIndependentJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly List<Timer> _timers = new List<Timer>();

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

                _semaphore = new SemaphoreSlim(1, 1);

                GetConfiguration(out SftpConfigModel config, out int prodTimeSpan, out int sbTimeSpan);

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
            prodTimeSpan = 10;
            sbTimeSpan = 15;
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
            }
        }

        private async Task RunFileCheckAndUpload(object state, bool isProduction, string productionOrSandboxFolder,
            SftpConfigModel config)
        {
            bool result;

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

                if (config.UseUnicode)
                    connectionInfo.Encoding = Encoding.UTF8;

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

                    result = await ProcessFileAndUpload(isProduction, productionOrSandboxFolder, files, connectionInfo);
                }
                else
                {
                    result = await ProcessFileAndUpload(isProduction, productionOrSandboxFolder, new List<string> { path }, connectionInfo);
                }

                if (result)
                {
                    _logger.LogInformation("File check and upload ran successfully!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + $" Error running file check and upload");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<bool> ProcessFileAndUpload(bool isProduction, string productionOrSandboxFolder, IEnumerable<string> files,
            ConnectionInfo connectionInfo)
        {
            try
            {
                return await FileHelpers.UploadFilesToSftp(files, isProduction, productionOrSandboxFolder, _serviceScopeFactory, _logger, connectionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message + " Error uploading file");
            }

            return false;
        }
    }
}