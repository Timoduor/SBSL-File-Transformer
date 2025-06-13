using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MySqlConnector;

using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;


namespace SbslFileTransformer.Infrastructure.Jobs.Others
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

            var timerClearOldUploadedFiles = new Timer(async state => await ClearOldUploadedFilesRecords(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            _timers.Add(timerClearOldUploadedFiles);

            var timerClearOldLogs = new Timer(async state => await ClearOldLogRecords(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            _timers.Add(timerClearOldLogs);

            var timerClearOldVisionRecords = new Timer(async state => await ClearOldVisionRecords(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            _timers.Add(timerClearOldVisionRecords);

            return Task.CompletedTask;
        }

        private async Task ClearOldLogRecords()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    using (var connection = new MySqlConnection(dbContext.Database.GetConnectionString()))
                    {
                        connection.Open();

                        var commandMaxDate = connection.CreateCommand();

                        commandMaxDate.CommandText =
                            $@"DELETE FROM Logs WHERE Timestamp < {DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd")}";

                        var deletedCount = commandMaxDate.ExecuteNonQuery();

                        _logger.LogInformation($"Deleted {deletedCount} old log records from the database.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
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
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running file Archive Job");

                //do it only at night
                if (DateTime.Now.Hour is >= 0 and <= 3)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        var memCache = scope.ServiceProvider.GetService<IMemoryCache>();

                        _ = memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                        var backUpPath = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder")).Value;

                        var productionFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "ProductionFolder")).Value;

                        var backUpAllFilesPeriod = await GetBackupAllFilesPeriod(dbContext);

                        var backUpUploadedFiles = await GetUploadedFilesArchiveMaxAge(dbContext);

                        _ = double.TryParse(backUpUploadedFiles, out var periodUploaded);

                        await DeleteOldUploadedFiles(dbContext, backUpPath, periodUploaded);

                        _ = memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                        _ = double.TryParse(backUpAllFilesPeriod, out var period);

                        ArchiveAllOldFiles(backUpPath, productionFolder, period);

                        _ = memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Completed);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {

                _ = _semaphore.Release();
            }
        }

        private static void ArchiveAllOldFiles(string backUpPath, string productionFolder, double period)
        {
            var searchOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive
            };

            foreach (var file in Directory.GetFiles(productionFolder, "*.*", searchOptions))
            {
                var props = new FileInfo(file);

                if (props.LastWriteTime < DateTime.Now.AddDays(-period))
                {
                    var destination = Path.Combine(backUpPath, Path.GetFileName(file));

                    File.Move(file, destination, true);
                }
            }
        }

        private static async Task DeleteOldUploadedFiles(ApplicationDbContext dbContext, string backUpPath, double periodUploaded)
        {
            var oldUploadedFiles = await
                                        dbContext.UploadedFiles.Where(f => f.UploadedDate < DateTime.Now.AddDays(-periodUploaded)).ToListAsync();

            foreach (var file in oldUploadedFiles)
            {
                var source = file.FilePath;
                var destination = Path.Combine(backUpPath, Path.GetFileName(file.FilePath));

                if (File.Exists(source))
                {
                    File.Move(source, destination, true);
                }
            }
        }

        private static async Task<string> GetUploadedFilesArchiveMaxAge(ApplicationDbContext dbContext)
        {
            var keyUploadedMaxAge = "UploadedFileArchiveMaxAge";
            var maxUploadedFileAge = "7";

            var backUpUploadedFiles = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                b.ConfigType == ConfigurationType.Setting && b.Key == keyUploadedMaxAge))?.Value;

            if (string.IsNullOrEmpty(backUpUploadedFiles))
            {
                _ = await dbContext.Configurations.AddAsync(new Configuration
                {
                    ConfigType = ConfigurationType.Setting,
                    Key = keyUploadedMaxAge,
                    Updated = DateTime.Now,
                    Value = maxUploadedFileAge
                });

                _ = await dbContext.SaveChangesAsync();

                backUpUploadedFiles = maxUploadedFileAge;
            }

            return backUpUploadedFiles;
        }

        private static async Task<string> GetBackupAllFilesPeriod(ApplicationDbContext dbContext)
        {
            var key = "ArchiveAllFilesOlderThanDays";
            var defaultPeriod = "30";

            var backUpAllFilesPeriod = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                b.ConfigType == ConfigurationType.Setting && b.Key == key))?.Value;

            if (string.IsNullOrEmpty(backUpAllFilesPeriod))
            {
                _ = await dbContext.Configurations.AddAsync(new Configuration
                {
                    ConfigType = ConfigurationType.Setting,
                    Key = key,
                    Updated = DateTime.Now,
                    Value = defaultPeriod
                });

                _ = await dbContext.SaveChangesAsync();

                backUpAllFilesPeriod = defaultPeriod;
            }

            return backUpAllFilesPeriod;
        }

        private async Task ClearTempFolder()
        {
            try
            {
                var tempFolder = await FileHelpers.GetTempPath(_serviceScopeFactory);

                //DELETE OLD BACKUPS 7 days or older
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
                    {
                        File.Delete(file);
                    }
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
                if (DateTime.Now.Hour is >= 0 and <= 1)
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

                    _ = memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                    connectionString = dbContext.Database.GetDbConnection().ConnectionString;

                    backUpFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                                       b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder"))
                                   ?.Value ??
                                   backUpFolder;
                }

                var backUpDirectory = PerformDBBackup(connectionString, backUpFolder);

                //DELETE OLD BACKUPS 7 days or older 
                _ = (memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Running));

                DeleteOldDBBackupFiles(backUpDirectory);

                _ = (memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Completed));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private static string PerformDBBackup(string connectionString, string backUpFolder)
        {
            var backUpDirectory = Path.Combine(backUpFolder, "SBSLETL_DB_Backup");

            _ = Directory.CreateDirectory(backUpDirectory);

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

            return backUpDirectory;
        }

        private static void DeleteOldDBBackupFiles(string backUpDirectory)
        {
            var searchOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive
            };

            foreach (var file in Directory.GetFiles(backUpDirectory, "*.*", searchOptions))
            {
                var props = new FileInfo(file);

                if (props.LastWriteTime < DateTime.Now.AddDays(-10) ||
                    props.CreationTime < DateTime.Now.AddDays(-10))
                {
                    File.Delete(file);
                }
            }
        }

        private async Task ClearOldUploadedFilesRecords()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var key = "UploadedFilesMaxAgeInDays";
                    var defaultAge = "365";

                    var configuration = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                        b.ConfigType == ConfigurationType.Setting && b.Key == key))?.Value;

                    if (string.IsNullOrEmpty(configuration))
                    {
                        _ = await dbContext.Configurations.AddAsync(new Configuration
                        {
                            ConfigType = ConfigurationType.Setting,
                            Key = key,
                            Updated = DateTime.Now,
                            Value = defaultAge
                        });

                        _ = await dbContext.SaveChangesAsync();

                        configuration = defaultAge;
                    }

                    var ageInDaysToClear = Convert.ToDouble(configuration);

                    var compareDate = DateTime.Now.AddDays(-ageInDaysToClear);

                    var uploadedFilesToRemove =
                        dbContext.UploadedFiles.Where(f => f.UploadedDate < compareDate);
                    var processedReportsToRemove =
                        dbContext.ProcessedReports.Where(f => f.ProcessedDate < compareDate);

                    dbContext.RemoveRange(uploadedFilesToRemove);
                    dbContext.RemoveRange(processedReportsToRemove);

                    _ = await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        private async Task ClearOldVisionRecords()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var key = "VisionRecordsMaxAgeInDays";
                    var defaultAge = "14";

                    var configuration = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                        b.ConfigType == ConfigurationType.Setting && b.Key == key))?.Value;

                    if (string.IsNullOrEmpty(configuration))
                    {
                        _ = await dbContext.Configurations.AddAsync(new Configuration
                        {
                            ConfigType = ConfigurationType.Setting,
                            Key = key,
                            Updated = DateTime.Now,
                            Value = defaultAge
                        });

                        _ = await dbContext.SaveChangesAsync();

                        configuration = defaultAge;
                    }

                    var ageInDaysToClear = Convert.ToDouble(configuration);

                    var compareDate = DateTime.Now.AddDays(-ageInDaysToClear);

                    var entitiesToRemove =
                        dbContext.VisionRecordCollections.Where(f => f.DateExtracted < compareDate);

                    dbContext.RemoveRange(entitiesToRemove);

                    _ = await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
