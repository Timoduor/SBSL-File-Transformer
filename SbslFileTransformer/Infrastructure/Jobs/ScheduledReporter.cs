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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Reporting
{
    public class ScheduledReporter : IHostedService
    {
        private readonly ILogger<ScheduledReporter> _logger;
        private readonly EmailSender _emailSender;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScheduledReporter(ILogger<ScheduledReporter> logger, EmailSender emailSender, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _emailSender = emailSender;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduled reporter job...");

            try
            {
                var config = GetConfiguration();

                var allReports = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/queryruns"; //get all reports
                //loop through them to see which report has nott been sent and send it

                foreach (var report in allReports)
                {
                    long reportId = 0;
                    string exportType = "";

                    var reportToSend = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/completedqueryrun/{reportId}/{exportType}";

                    //get the report and email it
                    //_emailSender.SendMessage()
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Scheduled reporter has been stopped");
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
                };

                return config;
            }
        }
    }
}
