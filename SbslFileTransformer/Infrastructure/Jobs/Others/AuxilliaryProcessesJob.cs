extern alias MySqlDataAlias;
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
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using MySqlBackup = MySql.Data.MySqlClient.MySqlBackup;
using MySqlCommand = MySqlDataAlias::MySql.Data.MySqlClient.MySqlCommand;
using MySqlConnection = MySqlDataAlias::MySql.Data.MySqlClient.MySqlConnection;


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
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _semaphore = new SemaphoreSlim(1, 1);

            Timer timer = new Timer(state => this.RestartService(), null, TimeSpan.Zero, TimeSpan.FromHours(2));
            this._timers.Add(timer);

            Timer timerArchive = new Timer(async state => await this.ArchiveOldFiles(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            this._timers.Add(timerArchive);

            Timer timerBackup = new Timer(async state => await this.BackupDb(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(1));
            this._timers.Add(timerBackup);

            Timer timerClearTemp = new Timer(async state => await this.ClearTempFolder(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(1));
            this._timers.Add(timerClearTemp);

            Timer timerClearOldUploadedFiles = new Timer(async state => await this.ClearOldUploadedFilesRecords(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            this._timers.Add(timerClearOldUploadedFiles);

            Timer timerClearOldVisionRecords = new Timer(async state => await this.ClearOldVisionRecords(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 600)), TimeSpan.FromHours(2));
            this._timers.Add(timerClearOldVisionRecords);

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Auxilliary Services stopped!");

            foreach (Timer timer in this._timers) 
                timer.Dispose();

            return Task.CompletedTask;
        }

        private async Task ArchiveOldFiles()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running file Archive Job");

                //do it only at night
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour <= 3)
                    using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                    {
                        ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        IMemoryCache memCache = scope.ServiceProvider.GetService<IMemoryCache>();

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Starting);

                        string backUpPath = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "BackUpFolder")).Value;

                        string productionFolder = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                            b.ConfigType == ConfigurationType.Sftp && b.Key == "ProductionFolder")).Value;

                        var backUpAllFilesPeriod = await GetBackupAllFilesPeriod(dbContext);

                        var backUpUploadedFiles = await GetUploadedFilesArchiveMaxAge(dbContext);

                        double.TryParse(backUpUploadedFiles, out double periodUploaded);

                        await DeleteOldUploadedFiles(dbContext, backUpPath, periodUploaded);
                       
                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                        double.TryParse(backUpAllFilesPeriod, out double period);

                        ArchiveAllOldFiles(backUpPath, productionFolder, period);

                        memCache.Set(nameof(AuxilliaryProcessesJob), JobState.Completed);
                    }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {

                _semaphore.Release();
            }
        }

        private static void ArchiveAllOldFiles(string backUpPath, string productionFolder, double period)
        {
            EnumerationOptions searchOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive
            };

            foreach (string file in Directory.GetFiles(productionFolder, "*.*", searchOptions))
            {
                FileInfo props = new FileInfo(file);

                if (props.LastWriteTime < DateTime.Now.AddDays(-period))
                {
                    string destination = Path.Combine(backUpPath, Path.GetFileName(file));

                    File.Move(file, destination, true);
                }
            }
        }

        private static async Task DeleteOldUploadedFiles(ApplicationDbContext dbContext, string backUpPath, double periodUploaded)
        {
            List<Models.SftpUploadedFile> oldUploadedFiles = await
                                        dbContext.UploadedFiles.Where(f => f.UploadedDate < DateTime.Now.AddDays(-periodUploaded)).ToListAsync();

            foreach (Models.SftpUploadedFile file in oldUploadedFiles)
            {
                string source = file.FilePath;
                string destination = Path.Combine(backUpPath, Path.GetFileName(file.FilePath));

                if (File.Exists(source))
                    File.Move(source, destination, true);
            }
        }

        private static async Task<string> GetUploadedFilesArchiveMaxAge(ApplicationDbContext dbContext)
        {
            string keyUploadedMaxAge = "UploadedFileArchiveMaxAge";
            string maxUploadedFileAge = "7";

            string backUpUploadedFiles = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                b.ConfigType == ConfigurationType.Setting && b.Key == keyUploadedMaxAge))?.Value;

            if (string.IsNullOrEmpty(backUpUploadedFiles))
            {
                await dbContext.Configurations.AddAsync(new Configuration
                {
                    ConfigType = ConfigurationType.Setting,
                    Key = keyUploadedMaxAge,
                    Updated = DateTime.Now,
                    Value = maxUploadedFileAge
                });

                await dbContext.SaveChangesAsync();

                backUpUploadedFiles = maxUploadedFileAge;
            }

            return backUpUploadedFiles;
        }

        private static async Task<string> GetBackupAllFilesPeriod(ApplicationDbContext dbContext)
        {
            string key = "ArchiveAllFilesOlderThanDays";
            string defaultPeriod = "30";

            string backUpAllFilesPeriod = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                b.ConfigType == ConfigurationType.Setting && b.Key == key))?.Value;

            if (string.IsNullOrEmpty(backUpAllFilesPeriod))
            {
                await dbContext.Configurations.AddAsync(new Configuration
                {
                    ConfigType = ConfigurationType.Setting,
                    Key = key,
                    Updated = DateTime.Now,
                    Value = defaultPeriod
                });

                await dbContext.SaveChangesAsync();

                backUpAllFilesPeriod = defaultPeriod;
            }

            return backUpAllFilesPeriod;
        }

        private async Task ClearTempFolder()
        {
            try
            {
                string tempFolder = await FileHelpers.GetTempPath(this._serviceScopeFactory);

                //DELETE OLD BACKUPS 7 days or older
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
                this._logger.LogError(ex, ex.Message);
            }
        }

        private void RestartService()
        {
            try
            {
                this._logger.LogInformation("Restarting SBSL Service");

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
                this._logger.LogError(ex, ex.Message);
            }
        }

        private async Task BackupDb()
        {
            try
            {
                string connectionString;

                string backUpFolder = @"C:\SBSLETL_DbBackup";

                IMemoryCache memCache;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
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

                var backUpDirectory = PerformDBBackup(connectionString, backUpFolder);

                //DELETE OLD BACKUPS 7 days or older 
                memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Running);

                DeleteOldDBBackupFiles(backUpDirectory);

                memCache?.Set(nameof(AuxilliaryProcessesJob), JobState.Completed);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }

        private static string PerformDBBackup(string connectionString, string backUpFolder)
        {
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

            return backUpDirectory;
        }

        private static void DeleteOldDBBackupFiles(string backUpDirectory)
        {
            EnumerationOptions searchOptions = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive
            };

            foreach (string file in Directory.GetFiles(backUpDirectory, "*.*", searchOptions))
            {
                FileInfo props = new FileInfo(file);

                if (props.LastWriteTime < DateTime.Now.AddDays(-7) ||
                    props.CreationTime < DateTime.Now.AddDays(-7))
                    File.Delete(file);
            }
        }

        private async Task ClearOldUploadedFilesRecords()
        {
            try
            {
                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    string key = "UploadedFilesMaxAgeInDays";
                    string defaultAge = "365";

                    string configuration = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                        b.ConfigType == ConfigurationType.Setting && b.Key == key))?.Value;

                    if (string.IsNullOrEmpty(configuration))
                    {
                        await dbContext.Configurations.AddAsync(new Configuration
                        {
                            ConfigType = ConfigurationType.Setting,
                            Key = key,
                            Updated = DateTime.Now,
                            Value = defaultAge
                        });

                        await dbContext.SaveChangesAsync();

                        configuration = defaultAge;
                    }

                    double ageInDaysToClear = Convert.ToDouble(configuration);

                    var compareDate = DateTime.Now.AddDays(-ageInDaysToClear);

                    IQueryable<SftpUploadedFile> uploadedFilesToRemove =
                        dbContext.UploadedFiles.Where(f => f.UploadedDate < compareDate);
                    IQueryable<ProcessedReport> processedReportsToRemove =
                        dbContext.ProcessedReports.Where(f => f.ProcessedDate < compareDate);

                    dbContext.RemoveRange(uploadedFilesToRemove);
                    dbContext.RemoveRange(processedReportsToRemove);

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }

        private async Task ClearOldVisionRecords()
        {
            try
            {
                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    string key = "VisionRecordsMaxAgeInDays";
                    string defaultAge = "14";

                    string configuration = (await dbContext.Configurations.FirstOrDefaultAsync(b =>
                        b.ConfigType == ConfigurationType.Setting && b.Key == key))?.Value;

                    if (string.IsNullOrEmpty(configuration))
                    {
                        await dbContext.Configurations.AddAsync(new Configuration
                        {
                            ConfigType = ConfigurationType.Setting,
                            Key = key,
                            Updated = DateTime.Now,
                            Value = defaultAge
                        });

                        await dbContext.SaveChangesAsync();

                        configuration = defaultAge;
                    }

                    double ageInDaysToClear = Convert.ToDouble(configuration);

                    var compareDate = DateTime.Now.AddDays(-ageInDaysToClear);

                    IQueryable<VisionRecordCollection> entitiesToRemove =
                        dbContext.VisionRecordCollections.Where(f => f.DateExtracted < compareDate);

                    dbContext.RemoveRange(entitiesToRemove);

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
        }
    }
}
