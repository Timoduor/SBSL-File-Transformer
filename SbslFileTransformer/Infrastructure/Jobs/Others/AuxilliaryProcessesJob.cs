extern alias MySqlDataAlias;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlDataAlias::MySql.Data.MySqlClient;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlBackup = MySql.Data.MySqlClient.MySqlBackup;


namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class AuxilliaryProcessesJob : IHostedService
    {
        private static SemaphoreSlim _semaphore;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AuxilliaryProcessesJob> _logger;

        private readonly List<Timer> _timers = new List<Timer>();

        public AuxilliaryProcessesJob(ILogger<AuxilliaryProcessesJob> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _semaphore = new SemaphoreSlim(1, 1);

            var timer = new Timer(state => RestartService(), null, TimeSpan.Zero, TimeSpan.FromHours(2));
            _timers.Add(timer);

            var timerArchive = new Timer(async state => await ArchiveOldFiles(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            _timers.Add(timerArchive);

            var timerBackup = new Timer(async state => await BackupDb(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(1));
            _timers.Add(timerBackup);

            var timerClearTemp = new Timer(async state => await ClearTempFolder(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(1));
            _timers.Add(timerClearTemp);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auxilliary Services stopped!");

            foreach (var timer in _timers) timer.Dispose();

            return Task.CompletedTask;
        }

        private async Task ArchiveOldFiles()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running file Archive Job");

                //do it only at night
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 3)
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        var memCache = scope.ServiceProvider.GetService<IMemoryCache>();

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                        var backUpPath = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder")).Value;

                        var productionFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "ProductionFolder")).Value;

                        var backUpAllFilesPeriod = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Setting && b.Key == "BackUpAllFilesPeriod")).Value;

                        var oldUploadedFiles = await
                            dbContext.UploadedFiles.Where(f => f.UploadedDate < DateTime.Now.AddDays(-7)).ToListAsync();

                        foreach (var file in oldUploadedFiles)
                        {
                            var source = file.FilePath;
                            var destination = Path.Combine(backUpPath, Path.GetFileName(file.FilePath));

                            if (File.Exists(source)) File.Move(source, destination, true);
                        }

                        var searchOptions = new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            MatchCasing = MatchCasing.CaseInsensitive
                        };


                        double.TryParse(backUpAllFilesPeriod, out double period);

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                        foreach (var file in Directory.GetFiles(productionFolder, "*.*", searchOptions))
                        {
                            var props = new FileInfo(file);

                            if (props.LastWriteTime < DateTime.Now.AddDays(-30))
                            {
                                var destination = Path.Combine(backUpPath, Path.GetFileName(file));

                                File.Move(file, destination, true);
                            }
                        }

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Completed);
                    }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                
                _semaphore.Release();
            }
        }

        private async Task ClearTempFolder()
        {
            try
            {
                var tempFolder = await FileHelpers.GetTempPath(_serviceScopeFactory);

                //DELETE OLD BACKUPS 2 days or older
                var searchOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                };

                foreach (var file in Directory.GetFiles(tempFolder, "*.*", searchOptions))
                {
                    var props = new FileInfo(file);

                    if (props.LastWriteTime < DateTime.Now.AddDays(-7) ||
                        props.CreationTime < DateTime.Now.AddDays(-7))
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private void RestartService()
        {
            try
            {
                _logger.LogInformation("Restarting SBSL Service");

                //do it only at night
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 3)
                {
#if (!DEBUG)
                    FileHelpers.RestartService();
#endif
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task BackupDb()
        {
            try
            {
                string connectionString;

                var backUpFolder = @"C:\SBSLETL_DbBackup";

                IMemoryCache memCache;

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    memCache = scope.ServiceProvider.GetService<IMemoryCache>();

                    memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                    connectionString = dbContext.Database.GetDbConnection().ConnectionString;

                    backUpFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                                       b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder"))
                                   ?.Value ??
                                   backUpFolder;
                }

                var backUpDirectory = Path.Combine(backUpFolder, "SBSLETL_DB_Backup");

                Directory.CreateDirectory(backUpDirectory);

                var backUpFile = Path.Combine(backUpDirectory, $"{DateTime.Now:yyyy_MM_dd_HH}.sql");

                using (var conn = new MySqlConnection(connectionString))
                {
                    using (var cmd = new MySqlCommand())
                    {
                        using (var mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportToFile(backUpFile);
                            conn.Close();
                        }
                    }
                }

                //DELETE OLD BACKUPS 2 days or older
                var searchOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                };

                memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                foreach (var file in Directory.GetFiles(backUpDirectory, "*.*", searchOptions))
                {
                    var props = new FileInfo(file);

                    if (props.LastWriteTime < DateTime.Now.AddDays(-2) ||
                        props.CreationTime < DateTime.Now.AddDays(-2))
                        File.Delete(file);
                }

                memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Completed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}