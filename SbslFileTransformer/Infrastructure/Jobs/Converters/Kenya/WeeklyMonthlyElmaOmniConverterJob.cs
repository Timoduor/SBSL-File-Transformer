using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Kenya;
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
using SbslFileTransformer.Converters.Kenya.Mpesa;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class WeeklyMonthlyElmaOmniConverterJob : ConverterJobBase<WeeklyMonthlyElmaOmniConverterJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(WeeklyMonthlyElmaOmniConverterJob);
        public WeeklyMonthlyElmaOmniConverterJob(ILogger<WeeklyMonthlyElmaOmniConverterJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _emailSender = emailSender;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Weekly Monthly Elma Omni Settlement Job");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await WeeklyElmaConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task WeeklyElmaConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running Weekly Monthly Elma Omni Settlement job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options)
                        .Where(f => f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".xlsx")).ToList();

                    files.AddRange(Directory.GetFiles(sbFolder, "*.*", options).Where(f =>
                        f.ToLower().EndsWith(".xls") || f.ToLower().EndsWith(".xlsx")));

                    WeeklyMonthlyElmaOmniSettlementConverter mpesaConverter = new WeeklyMonthlyElmaOmniSettlementConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("utilities") && !file.ToLower().Contains("daily") &&
                            !file.Contains("Conv")
                            && file.ToLower().Contains("imke") &&
                            (file.ToLower().Contains("elma") || file.ToLower().Contains("omni")))
                        {
                            SftpUploadedFile fileToProcess =
                                uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                                try
                                {
                                    mpesaConverter.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    await ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    CompleteFileProcessing(updatedFiles, fileToProcess, nameof(WeeklyMonthlyElmaOmniSettlementConverter));
                                }
                        }
                        await SaveProcessedFilesStatuses(dbContext, updatedFiles);
                    }
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
    }
}