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
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya.Ecommerce
{
    /// <summary>
    /// Compares the balances with the summation from the extracted transactions in the database
    /// and if they match this creates a file for the transactions to processed further
    /// </summary>
    public class EcommerceMatchingJob : ConverterJobBase<EcommerceMatchingJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(EcommerceMatchingJob);

        private IMemoryCache _memoryCache;

        public EcommerceMatchingJob(ILogger<EcommerceMatchingJob> logger,
            IServiceScopeFactory serviceScopeFactory, EmailSender emailSender, JobDisplayManager jobManager, IMemoryCache memoryCache)
        {
            this._logger = logger;
            this._serviceScopeFactory = serviceScopeFactory;
            this._emailSender = emailSender;
            this._jobManager = jobManager;
            _memoryCache = memoryCache;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Ecommerce Matching Job");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.EcommerceMatcher(), null,
                TimeSpan.FromSeconds(new Random().Next(30, 100)), TimeSpan.FromMinutes(15));

            return Task.CompletedTask;
        }

        private async Task EcommerceMatcher()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running Ecommerce Record Matcher job");

                string prodFolder = string.Empty;
                string sbFolder = string.Empty;
                string Entity = string.Empty;

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    this.CurrentJobStatus = this._jobManager.GetJobStatus(JobName);

                    if (this.CurrentJobStatus == null)
                    {
                        this.CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Running };

                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    }

                    List<Configuration> configurations = dbContext.Configurations.ToList();

                    Entity = configurations
                        .FirstOrDefault(c => c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;
                    prodFolder = configurations.FirstOrDefault(c => c.Key == "ProductionFolder")?.Value;
                    sbFolder = configurations.FirstOrDefault(c => c.Key == "SandboxFolder")?.Value;


                    EnumerationOptions options = new EnumerationOptions
                    { RecurseSubdirectories = true, MatchCasing = MatchCasing.CaseInsensitive };

                    List<string> files = Directory.GetFiles(prodFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv"))
                       .ToList();

                    files.AddRange(
                        Directory.GetFiles(sbFolder, "*.*", options).Where(f => f.ToLower().EndsWith(".csv")));

                    var mpesaConverter = new EcommerceRecordMatcher(_serviceScopeFactory, _logger, _emailSender);

                    List<SftpUploadedFile> uploadedFiles = await dbContext.UploadedFiles.ToListAsync();

                    List<SftpUploadedFile> updatedFiles = new List<SftpUploadedFile>();

                    this.CurrentJobStatus.Status = JobState.Running;
                    this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);

                    int count = 0;
                    int total = files.Count;

                    if (_memoryCache.TryGetValue("EcommerceExtractorLock", out bool isExtractorRunning))
                    {
                        while (isExtractorRunning)
                        {
                            await Task.Delay(5000);
                            _memoryCache.TryGetValue("EcommerceExtractorLock", out isExtractorRunning);
                            _logger.LogInformation($"Waiting for {"EcommerceExtractorLock".ToUpper()} to complete...");
                        }
                    }

                    foreach (string file in files)
                    {
                        //SHOULD BE UPDATED TO CORRECT FILE FINACLE PATH
                        if (
                            file.ToLower().Contains("cards") && file.ToLower().Contains("imke") &&
                            (file.ToLower().Contains("ecom_finacle"))
                           )
                        {
                            SftpUploadedFile fileToProcess = uploadedFiles.FirstOrDefault(f =>
                                     f.FilePath.ToLower() == file.ToLower());

                            if (fileToProcess != null)// not checked for processed because new records might be added that match
                            {

                                string path = Path.GetDirectoryName(file);
                                string outputPath = Path.Combine(Path.GetFullPath(Path.Combine(path, @"..\")), "Col_Conv");

                                Directory.CreateDirectory(outputPath);

                                try
                                {
                                    await mpesaConverter.MatchFiles(file, outputPath);
                                }
                                catch (Exception ex)
                                {
                                    await this.ProcessFileFailure(configurations, file, fileToProcess, ex);
                                }
                                finally
                                {
                                    this.CompleteFileProcessing(updatedFiles, fileToProcess, nameof(EcommerceMatchingJob));
                                }
                            }
                        }
                        this.CurrentJobStatus.ProgressMessage = $"Currently processing {file}... {count} of {total}";
                        this.CurrentJobStatus.SetProgress(count, total);
                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    }
                    await this.SaveProcessedFilesStatuses(dbContext, updatedFiles);

                    this.CurrentJobStatus.Status = JobState.Completed;
                    this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);

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
