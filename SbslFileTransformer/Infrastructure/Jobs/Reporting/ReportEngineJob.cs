using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting
{
    public class ReportEngineJob : ConverterJobBase<ReportEngineJob>, IHostedService
    {
        protected override string JobName { get; set; } = nameof(ReportEngineJob);
        public ReportEngineJob(ILogger<ReportEngineJob> logger, EmailSender emailSender,
            IServiceScopeFactory serviceScopeFactory, IHttpClientFactory httpClientFactory, JobDisplayManager jobManager)
        {
            _logger = logger;
            _emailSender = emailSender;
            _serviceScopeFactory = serviceScopeFactory;
            _jobManager = jobManager;
            HttpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduled reporter job...");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ProcessNewReports(), null,
                TimeSpan.FromSeconds(new Random().Next(15, 30)), TimeSpan.FromMinutes(15));

            return Task.CompletedTask;
        }

        private async Task ProcessNewReports()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running reporting job...");

                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    Entity = dbContext.Configurations.FirstOrDefault(c =>
                                c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                    CurrentJobStatus = _jobManager.GetJobStatus(JobName);

                    if (CurrentJobStatus == null)
                    {
                        CurrentJobStatus = new JobStatus(JobName) { Status = JobState.Starting };

                        _jobManager.SetJobStatus(JobName, CurrentJobStatus);
                    }

                    Dictionary<string, IEnumerable<ReportModel>> unprocessedReports = await FetchReports(dbContext);

                    await ProcessReports(unprocessedReports);
                }

                CurrentJobStatus.Status = JobState.Completed;
                _jobManager.SetJobStatus(JobName, CurrentJobStatus);
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

        private ReportConfigModel LoadReportConnectionConfig()
        {
            _logger.LogInformation("Fetching report connection configuration");

            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                List<Configuration> reportConfigurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report)
                    .ToList();

                List<Configuration> userLogins = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.ReportUser)
                    .ToList();

                ReportConfigModel config = new ReportConfigModel
                {
                    BaseUrl = reportConfigurations.FirstOrDefault(c => c.Key == "BaseUrl")?.Value,
                    EnvironmentUrl = reportConfigurations.FirstOrDefault(c => c.Key == "EnvironmentUrl")?.Value,
                    UserToken = reportConfigurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                    EmailBody = reportConfigurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                    EmailHeader = reportConfigurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                    ExportType = reportConfigurations.FirstOrDefault(c => c.Key == "ExportType")?.Value,
                    Scope = reportConfigurations.FirstOrDefault(c => c.Key == "Scope")?.Value,
                    TokenUrl = reportConfigurations.FirstOrDefault(c => c.Key == "TokenUrl")?.Value,
                    ClientId = reportConfigurations.FirstOrDefault(c => c.Key == "ClientId")?.Value,
                    ClientSecret = reportConfigurations.FirstOrDefault(c => c.Key == "ClientSecret")?.Value
                };

                config.UserNamesAndPasswords = new Dictionary<string, string>();

                foreach (Configuration login in userLogins)
                {
                    config.UserNamesAndPasswords.Add(login.Key, login.Value);
                }

                return config;
            }
        }

        private async Task<Dictionary<string, IEnumerable<ReportModel>>> FetchReports(ApplicationDbContext dbContext)
        {
            ReportFetcher reportFetcher = new ReportFetcher(_logger, HttpClientFactory, LoadReportConnectionConfig());

            List<ProcessedReport> processedReports = dbContext.ProcessedReports.ToList();

            IProgress<int> fetchReportProgress = new Progress<int>(percent =>
            {
                CurrentJobStatus.ProgressMessage = $"Fetching unprocessed reports... {percent} %";
                CurrentJobStatus.SetProgress(percent, 100);
                _jobManager.SetJobStatus(JobName, CurrentJobStatus);
            });

            return await reportFetcher.GetAllUnprocessedRecentReportsAsync(processedReports, fetchReportProgress);
        }

        private async Task ProcessReports(Dictionary<string, IEnumerable<ReportModel>> unprocessedReports)
        {
            ReportProcessor reportProcessor = new ReportProcessor(_logger, _serviceScopeFactory, HttpClientFactory, LoadReportConnectionConfig());

            IProgress<int> processReportProgress = new Progress<int>(percent =>
            {
                CurrentJobStatus.ProgressMessage = $"Processing unprocessed reports... {percent} %";
                CurrentJobStatus.SetProgress(percent, 100);
                _jobManager.SetJobStatus(JobName, CurrentJobStatus);
            });

            await reportProcessor.ProcessReports(unprocessedReports, Entity, processReportProgress);
        }


    }
}
