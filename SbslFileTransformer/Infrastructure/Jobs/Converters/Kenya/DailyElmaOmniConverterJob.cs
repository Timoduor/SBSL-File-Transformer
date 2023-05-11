using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Kenya.Mpesa;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class DailyElmaOmniConverterJob : ConverterJobBase<DailyElmaOmniConverterJob>, IHostedService
    {
        public DailyElmaOmniConverterJob(ILogger<DailyElmaOmniConverterJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
        }

        protected override string JobName { get; set; } = nameof(DailyElmaOmniConverterJob);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Daily Elma Omni Settlement Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.MonthlyOmniConverter(), null,
                TimeSpan.FromSeconds(new Random().Next(60, 200)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task MonthlyOmniConverter()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running Daily Elma Omni Settlement job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
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

                    DailyElmaOmniSettlementConverter mpesaConverter = new DailyElmaOmniSettlementConverter();

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    foreach (string file in files)
                    {
                        if (file.ToLower().Contains("utilities") && file.ToLower().Contains("imke") &&
                            !file.Contains("Conv")
                            && (file.ToLower().Contains("omni") || file.ToLower().Contains("elma")) &&
                            file.ToLower().Contains("daily"))
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f =>
                                    f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null && fileToProcess.Converted == false)
                            {
                                try
                                {
                                    mpesaConverter.ConvertFile(file);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(DailyElmaOmniSettlementConverter));
                                }
                            }
                        }
                    }
                    await this.SaveProcessedFilesStatuses(dbContext, updatedFiles);
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
    }
}