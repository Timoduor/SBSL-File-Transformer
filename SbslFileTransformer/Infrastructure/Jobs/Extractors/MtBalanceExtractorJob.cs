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
using SbslFileTransformer.Models.ViewModels;

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
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting MT Balance Extractor...");

            this.GetConfiguration(out SftpConfigModel config);

            int loopTime = 7;

            if (config.IncludeProduction)
            {
                string statementFolderProd =
                    Path.Combine(config.ProductionFolder,
                        @$"{config.Entity}\NOSTRO\STATEMENT"); //TODO PUT IN CONFIG OR CHANGE FOR DIFFERENT COUNTRIES

                if (!Directory.Exists(statementFolderProd)) Directory.CreateDirectory(statementFolderProd);

                //sync all folders every hours
                Timer timerProduction = new Timer(async state =>
                        await MTFileConverter.RunMTBalanceExtractor(statementFolderProd, config.ProductionFolder, this._serviceScopeFactory, this._logger)
                    , null, TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(loopTime));

                this._timers.Add(timerProduction);
            }

            else //if (config.IncludeSandbox)
            {
                string statementFolder =
                    Path.Combine(config.SandboxFolder,
                        @$"{config.Entity}\NOSTRO\STATEMENT"); //TODO PUT IN CONFIG OR CHANGE FOR DIFFERENT COUNTRIES

                if (!Directory.Exists(statementFolder)) Directory.CreateDirectory(statementFolder);

                Timer timerSandbox = new Timer(async state =>
                        await MTFileConverter.RunMTBalanceExtractor(statementFolder, config.SandboxFolder, this._serviceScopeFactory, this._logger)
                    , null, TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(loopTime));

                this._timers.Add(timerSandbox);
            }

            Timer timedValidator = new Timer(
                async state =>
                    await MTFileConverter.RunMtSequenceValidationCheck(this._serviceScopeFactory, this._logger, this._emailSender),
                null, TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromHours(12)); //TODO

            this._timers.Add(timedValidator);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("MT Balance Extractor stopped");

            foreach (Timer timer in this._timers)
            {
                timer?.Change(Timeout.Infinite, 0);
                await timer.DisposeAsync();
            }
        }

        private void GetConfiguration(out SftpConfigModel config)
        {
            using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                dbContext.Database.EnsureCreated();

                List<Configuration> configurations = dbContext.Configurations.Where(c =>
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