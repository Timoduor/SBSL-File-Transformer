using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class ScheduledReporter : IHostedService
    {
        private readonly ILogger<ScheduledReporter> _logger;
        private readonly EmailSender _emailSender;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;

        public ScheduledReporter(ILogger<ScheduledReporter> logger, EmailSender emailSender, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _emailSender = emailSender;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduled reporter job...");

            //_timer = new Timer((state) => ProcessNewReport().GetAwaiter().GetResult(), null, TimeSpan.Zero, TimeSpan.FromHours(12));

            return Task.CompletedTask;
        }

        private async Task ProcessNewReport()
        {
            try
            {
                var config = GetConfiguration();

                var allReports = GetRecentReports(config);

                foreach (var report in allReports)
                {
                    var savedFile = DownloadReport(0, config, Path.Combine(Path.GetTempPath(), "SBSL", report));

                    var results = ProcessReportFile(savedFile);

                    foreach (var key in results)
                    {
                        var emails = GetEmails(key.Key);

                        await _emailSender.SendMessage(emails, config.EmailHeader, config.EmailBody, filePaths: key.Value);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        private string[] GetRecentReports(ReportConfigModel config)
        {
            var reportsUrl = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/queryruns"; //get all reports
                                                                            //loop through them to see which report has nott been sent and send it

            throw new NotImplementedException();
        }

        private IEnumerable<string> GetEmails(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="savedFile"></param>
        /// <returns>List of key: email group name and value: list of files to send to them</returns>
        private List<KeyValuePair<string, List<string>>> ProcessReportFile(string savedFile)
        {
            throw new NotImplementedException();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Scheduled reporter has been stopped");
            await _timer.DisposeAsync();
        }

        private ReportConfigModel GetConfiguration()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report).ToList();

                var config = new ReportConfigModel
                {
                    BaseUrl = configurations.FirstOrDefault(c => c.Key == "BaseUrl")?.Value,
                    EnvironmentUrl = configurations.FirstOrDefault(c => c.Key == "EnvironmentUrl")?.Value,
                    UserName = configurations.FirstOrDefault(c => c.Key == "UserName")?.Value,
                    Password = !string.IsNullOrEmpty(configurations.FirstOrDefault(c => c.Key == "Password")?.Value) ?
                                                    encryptionManager.Decrypt(configurations.FirstOrDefault(c => c.Key == "Password")?.Value) : "",
                    UserToken = configurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                    EmailBody = configurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                    EmailHeader = configurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                    ExportType = configurations.FirstOrDefault(c => c.Key == "ExportType")?.Value,
                };

                return config;
            }
        }

        private string DownloadReport(long reportId, ReportConfigModel config, string savePath)
        {
            var reportToDownload = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/completedqueryrun/{reportId}/{config.ExportType}";

            var net = new System.Net.WebClient();
            var data = net.DownloadData(reportToDownload);

            File.WriteAllBytes(savePath, data);

            return savePath;
        }
    }
}
