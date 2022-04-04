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

            Timer timer = new Timer(state => RestartService(), null, TimeSpan.Zero, TimeSpan.FromHours(2));
            _timers.Add(timer);

            Timer timerArchive = new Timer(async state => await ArchiveOldFiles(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            _timers.Add(timerArchive);

            Timer timerBackup = new Timer(async state => await BackupDb(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(1));
            _timers.Add(timerBackup);

            Timer timerClearTemp = new Timer(async state => await ClearTempFolder(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(1));
            _timers.Add(timerClearTemp);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Auxilliary Services stopped!");

            foreach (Timer timer in _timers) timer.Dispose();

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
                    using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                    {
                        ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        IMemoryCache memCache = scope.ServiceProvider.GetService<IMemoryCache>();

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                        string backUpPath = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder")).Value;

                        string productionFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "ProductionFolder")).Value;

                        string backUpAllFilesPeriod = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Setting && b.Key == "BackUpAllFilesPeriod")).Value;

                        List<Models.SftpUploadedFile> oldUploadedFiles = await
                            dbContext.UploadedFiles.Where(f => f.UploadedDate < DateTime.Now.AddDays(-7)).ToListAsync();

                        foreach (Models.SftpUploadedFile file in oldUploadedFiles)
                        {
                            string source = file.FilePath;
                            string destination = Path.Combine(backUpPath, Path.GetFileName(file.FilePath));

                            if (File.Exists(source)) File.Move(source, destination, true);
                        }

                        EnumerationOptions searchOptions = new EnumerationOptions
                        {
                            RecurseSubdirectories = true,
                            MatchCasing = MatchCasing.CaseInsensitive
                        };


                        double.TryParse(backUpAllFilesPeriod, out double period);

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                        foreach (string file in Directory.GetFiles(productionFolder, "*.*", searchOptions))
                        {
                            FileInfo props = new FileInfo(file);

                            if (props.LastWriteTime < DateTime.Now.AddDays(-30))
                            {
                                string destination = Path.Combine(backUpPath, Path.GetFileName(file));

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
                string tempFolder = await FileHelpers.GetTempPath(_serviceScopeFactory);

                //DELETE OLD BACKUPS 2 days or older
                EnumerationOptions searchOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                };

                foreach (string file in Directory.GetFiles(tempFolder, "*.*", searchOptions))
                {
                    FileInfo props = new FileInfo(file);

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
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 1)
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

                string backUpFolder = @"C:\SBSLETL_DbBackup";

                IMemoryCache memCache;

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    memCache = scope.ServiceProvider.GetService<IMemoryCache>();

                    memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                    connectionString = dbContext.Database.GetDbConnection().ConnectionString;

                    backUpFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                                       b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder"))
                                   ?.Value ??
                                   backUpFolder;
                }

                string backUpDirectory = Path.Combine(backUpFolder, "SBSLETL_DB_Backup");

                Directory.CreateDirectory(backUpDirectory);

                string backUpFile = Path.Combine(backUpDirectory, $"{DateTime.Now:yyyy_MM_dd_HH}.sql");

                using (var conn = new MySqlConnection(connectionString))
                {
                    using (var cmd = new MySqlCommand())
                    {
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            cmd.Connection = conn;
                            conn.Open();
                            mb.ExportToFile(backUpFile);
                            conn.Close();
                        }
                    }
                }

                //DELETE OLD BACKUPS 2 days or older
                EnumerationOptions searchOptions = new EnumerationOptions
                {
                    RecurseSubdirectories = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                };

                memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                foreach (string file in Directory.GetFiles(backUpDirectory, "*.*", searchOptions))
                {
                    FileInfo props = new FileInfo(file);

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