using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Extractors
{
    public class MtBalanceExtractorJob : IHostedService
    {
        private readonly EmailSender _emailSender;
        private readonly ILogger<MtBalanceExtractorJob> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly List<Timer> _timers = new List<Timer>();

        public MtBalanceExtractorJob(ILogger<MtBalanceExtractorJob> logger, IServiceScopeFactory serviceScopeFactory,
            EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MT Balance Extractor...");

            SftpConfigModel config;

            GetConfiguration(out config);

            var loopTime = 10;

            if (config.IncludeProduction)
            {
                var statementFolderProd =
                    Path.Combine(config.ProductionFolder,
                        @$"{config.Entity}\NOSTRO\STATEMENT"); //TODO PUT IN CONFIG OR CHANGE FOR DIFFERENT COUNTRIES

                if (!Directory.Exists(statementFolderProd)) Directory.CreateDirectory(statementFolderProd);

                //sync all folders every hours
                var timerProduction = new Timer(async state =>
                        await MTFileConverter.RunMTBalanceExtractor(statementFolderProd, true, config.ProductionFolder,
                            _serviceScopeFactory, _logger)
                    , null, TimeSpan.FromSeconds(new Random().Next(5, 30)), TimeSpan.FromMinutes(loopTime));

                _timers.Add(timerProduction);
            }

            else //if (config.IncludeSandbox)
            {
                var statementFolder =
                    Path.Combine(config.SandboxFolder,
                        @$"{config.Entity}\NOSTRO\STATEMENT"); //TODO PUT IN CONFIG OR CHANGE FOR DIFFERENT COUNTRIES

                if (!Directory.Exists(statementFolder)) Directory.CreateDirectory(statementFolder);

                var timerSandbox = new Timer(async state =>
                        await MTFileConverter.RunMTBalanceExtractor(statementFolder, false, config.SandboxFolder,
                            _serviceScopeFactory, _logger)
                    , null, TimeSpan.FromSeconds(new Random().Next(5, 30)), TimeSpan.FromMinutes(loopTime));

                _timers.Add(timerSandbox);
            }

            var timedValidator = new Timer(
                async state =>
                    await MTFileConverter.RunMtSequenceValidationCheck(_serviceScopeFactory, _logger, _emailSender),
                null, TimeSpan.FromSeconds(new Random().Next(10, 60)), TimeSpan.FromHours(12)); //TODO

            _timers.Add(timedValidator);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MT Balance Extractor stopped");

            foreach (var timer in _timers)
            {
                timer?.Change(Timeout.Infinite, 0);
                await timer.DisposeAsync();
            }
        }

        private void GetConfiguration(out SftpConfigModel config)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                dbContext.Database.EnsureCreated();

                var configurations = dbContext.Configurations.Where(c =>
                    c.ConfigType == ConfigurationType.Sftp || c.ConfigType == ConfigurationType.Setting).ToList();

                config = new SftpConfigModel
                {
                    Host = configurations.FirstOrDefault(c => c.Key == "Host")?.Value,
                    Port = Convert.ToInt32(configurations.FirstOrDefault(c => c.Key == "Port")?.Value),
                    UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                    //Password = _encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value),
                    RecurseFolders =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "RecurseFolders")?.Value),
                    IncludeSandbox =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeSandbox")?.Value),
                    IncludeProduction =
                        Convert.ToBoolean(configurations.FirstOrDefault(c => c.Key == "IncludeProduction")?.Value),
                    ProductionFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value,
                    SandboxFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value,
                    Entity = configurations
                        .FirstOrDefault(c => c.Key == "Entity" && c.ConfigType == ConfigurationType.Setting)?.Value
                };
            }
        }
    }
}