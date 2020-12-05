using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Files;
using SbslFileTransformer.Infrastructure.Sftp;
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
        private ILogger<InputFileWatcher> _fileLogger;

        Timer _timer;

        public SftpIndependentJob(IServiceScopeFactory serviceScopeFactory, ILogger<SftpIndependentJob> logger, ILogger<InputFileWatcher> fileLogger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _fileLogger = fileLogger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //sync all folders every hours
            _timer = new Timer(async(state) => await RunFileCheckAndUpload(state), null, TimeSpan.Zero,
            TimeSpan.FromMinutes(60));

            SftpConfigModel config;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Sftp).ToList();

                config = new SftpConfigModel
                {
                    Host = configurations.First(c => c.Key == "Host").Value,
                    Port = Convert.ToInt32(configurations.First(c => c.Key == "Port").Value),
                    UserName = configurations.First(c => c.Key == "UserName").Value,
                    Password = configurations.First(c => c.Key == "Password").Value,
                    RecurseFolders = Convert.ToBoolean(configurations.First(c => c.Key == "RecurseFolders").Value),
                    IncludeSandbox = Convert.ToBoolean(configurations.First(c => c.Key == "IncludeSandbox").Value),
                    IncludeProduction = Convert.ToBoolean(configurations.First(c => c.Key == "IncludeProduction").Value),
                    ProductionFolder = configurations.First(c => c.Key == "ProductionFolder").Value,
                    SandboxFolder = configurations.First(c => c.Key == "SandboxFolder").Value,
                };
            }

            if (config.IncludeProduction)
            {
                var fileWatcher = new InputFileWatcher(config.ProductionFolder, _fileLogger);

                fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess);
            }

            if (config.IncludeSandbox)
            {
                var fileWatcher = new InputFileWatcher(config.SandboxFolder, _fileLogger);

                fileWatcher.ProcessFile = async fileToProcess => await RunFileCheckAndUpload(fileToProcess);
            }
        }


        private async Task RunFileCheckAndUpload(object state)
        {
            var path = state?.ToString();

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !File.Exists(path))
            {
                //do check for all folders/files
            }
            else
            {
                //do check for specific file
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
