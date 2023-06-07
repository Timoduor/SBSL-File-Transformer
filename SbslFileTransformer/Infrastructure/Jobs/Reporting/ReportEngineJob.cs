using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers.Interfaces;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting
{
    public class ReportEngineJob : ConverterJobBase<ReportEngineJob>, IHostedService
    {
        private IReportsDownloader _reportsDownloader;
        IReportProcessor _reportProcessor;

        protected override string JobName { get; set; } = nameof(ReportEngineJob);
        public ReportEngineJob(ILogger<ReportEngineJob> logger, EmailSender emailSender,
            IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory, JobDisplayManager jobManager, IReportsDownloader reportsDownloader, IReportProcessor reportProcessor)
        {
            this._logger = logger;
            this._emailSender = emailSender;
            this._serviceScopeFactory = serviceScopeFactory;
            this._jobManager = jobManager;
            this.HttpClientFactory = httpClientFactory;
            this._reportsDownloader = reportsDownloader;
            this._reportProcessor = reportProcessor;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._logger.LogInformation("Starting Scheduled reporter job...");

            _semaphore = new SemaphoreSlim(1, 1);

            this._timer = new Timer(async state => await this.ProcessNewReports(), null,
                TimeSpan.FromSeconds(new Random().Next(15, 30)), TimeSpan.FromMinutes(15));

            return Task.CompletedTask;
        }

        private async Task ProcessNewReports()
        {
            try
            {
                await _semaphore.WaitAsync();

                this._logger.LogInformation("Running reporting job...");

                using (IServiceScope scope = this._serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    Entity = dbContext.Configurations.FirstOrDefault(c =>
                                c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                    this.CurrentJobStatus = this._jobManager.GetJobStatus(JobName);

                    if (this.CurrentJobStatus == null)
                    {
                        this.CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Starting };

                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    }

                    IProgress<int> fetchReportProgress = new Progress<int>(percent =>
                    {
                        this.CurrentJobStatus.ProgressMessage = $"Fetching unprocessed reports... {percent} %";
                        this.CurrentJobStatus.SetProgress(percent, 100);
                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    });

                    IProgress<int> processReportProgress = new Progress<int>(percent =>
                    {
                        this.CurrentJobStatus.ProgressMessage = $"Processing fetched reports... {percent} %";
                        this.CurrentJobStatus.SetProgress(percent, 100);
                        this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
                    });

                    List<long> processedReportsIds = dbContext.ProcessedReports.Select(r => r.ReportId).ToList();

                    Dictionary<string, IEnumerable<ReportModel>> unprocessedReportList = await this._reportsDownloader.GetUnprocessedReportListAsync(processedReportsIds, fetchReportProgress);

                    _logger.LogInformation($"Found {unprocessedReportList.Count} unprocessed reports.");

                    await this._reportProcessor.ProcessFetchedReportsAsync(unprocessedReportList, processReportProgress);
                }

                this.CurrentJobStatus.Status = JobState.Completed;
                this._jobManager.SetJobStatus(JobName, this.CurrentJobStatus);
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
