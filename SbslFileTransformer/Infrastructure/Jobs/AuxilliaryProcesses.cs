using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class AuxilliaryProcesses : IHostedService
    {
        ILogger<AuxilliaryProcesses> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        List<Timer> _timers = new List<Timer>();
        public AuxilliaryProcesses(ILogger<AuxilliaryProcesses> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var timer = new Timer((state) => RestartService(), null, TimeSpan.Zero, TimeSpan.FromHours(2));
            _timers.Add(timer);

            var timerArchive = new Timer(async (state) => await ArchiveOldFiles(), null, TimeSpan.Zero, TimeSpan.FromHours(2));
            _timers.Add(timerArchive);

            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auxilliary Services stopped!");

            foreach (var timer in _timers)
            {
                timer.Dispose();
            }

            return Task.CompletedTask;
        }

        private async Task ArchiveOldFiles()
        {
            try
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 7)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        var backUpPath = (await dbContext.Configurations.FirstOrDefaultAsync(b => b.ConfigType == Models.Enums.ConfigurationType.Sftp && b.Key == "BackUpFolder")).Value;

                        var productionFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b => b.ConfigType == Models.Enums.ConfigurationType.Sftp && b.Key == "ProductionFolder")).Value;

                        var oldUploadedFiles = dbContext.UploadedFiles.Where(f => f.UploadedDate < DateTime.Now.AddDays(-5));

                        foreach (var file in oldUploadedFiles)
                        {
                            if (File.Exists(file.FilePath))
                            {
                                File.Move(file.FilePath, Path.Combine(backUpPath, Path.GetFileName(file.FilePath)));
                            }
                        }

                        var searchOptions = new EnumerationOptions
                        {
                            RecurseSubdirectories = true
                        };

                        foreach(var file in Directory.GetFiles(productionFolder, "*.*", searchOptions))
                        {
                            var props = new FileInfo(file);

                            if(props.LastWriteTime < DateTime.Now.AddDays(-7) || props.CreationTime < DateTime.Now.AddDays(-7))
                            {
                                File.Move(file, Path.Combine(backUpPath, Path.GetFileName(file)));
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void RestartService()
        {
            try
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 4)
                {
                    StaticHelpers.RestartService("SBSL ETL Service");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }


    }
}
