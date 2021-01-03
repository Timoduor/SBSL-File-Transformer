using CsvHelper;
using ExcelDataReader;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

            _timer = new Timer((state) => ProcessNewReport().GetAwaiter().GetResult(), null, TimeSpan.Zero, TimeSpan.FromHours(12));

            return Task.CompletedTask;
        }

        private async Task ProcessNewReport()
        {
            try
            {
                var config = GetConfiguration();

                //FOR TEST PURPOSES ONLY
                {
                    var testResults = ProcessReportFile(@"C:\Users\Yida\Downloads\CBK Open Items Daily Report (8).xlsx");

                    foreach (var key in testResults)
                    {
                        //key is the overdue days used to select the email groups
                        var emails = GetEmails(key.Key);

                        await _emailSender.SendMessage(emails, $"Overdue recons by {key.Key} days", $"This is a report for reconciliations overdue by {key.Key} days", filePaths: new string[] { key.Value });
                    }
                }


                var allReports = GetRecentReports(config);



                foreach (var report in allReports)
                {
                    var savedFile = DownloadReport(0, config, Path.Combine(Path.GetTempPath(), "SBSL", report));

                    var results = ProcessReportFile(savedFile);

                    foreach (var key in results)
                    {
                        //key is the overdue days used to select the email groups
                        var emails = GetEmails(key.Key);

                        await _emailSender.SendMessage(emails, config.EmailHeader, config.EmailBody, filePaths: new string[] { key.Value });
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

        private IEnumerable<string> GetEmails(int key)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var groups = dbContext.EmailGroups.Where(g => g.AgeAlertDuration >= key);

                return groups.Select(g => g.Emails).ToList();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="savedFile"></param>
        /// <returns>List of key: email group name and value: list of files to send to them</returns>
        private Dictionary<int, string> ProcessReportFile(string inputFile)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var daysRecordsPairs = new Dictionary<int, List<OpenItem>>();

            var openItems = new List<OpenItem>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    string lastAccountNo = string.Empty;

                    while (reader.Read())
                    {
                        var col3 = reader.GetValue(3)?.ToString();
                        if (string.IsNullOrEmpty(col3))
                        {
                            continue;
                        }

                        DateTime postedDate;

                        if (DateTime.TryParseExact(col3, "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out postedDate))
                        {
                            var openItem = new OpenItem
                            {
                                DaysOverdue = (DateTime.Now - postedDate).TotalDays,
                                PostedDate = postedDate,

                                AccName = reader.GetValue(2)?.ToString(),
                                Account = lastAccountNo,
                                ActiveCertStatus = reader.GetValue(14)?.ToString(),
                                Amount = reader.GetValue(4).ToString().Contains("(") ? Convert.ToDouble(reader.GetValue(4)?.ToString().Trim('(', ')')) * -1 : Convert.ToDouble(reader.GetValue(4)?.ToString().Trim('(', ')')),
                                Entity = reader.GetValue(1).ToString(),
                                FunctionalArea = reader.GetValue(13).ToString(),
                                ItemId = Convert.ToInt32(reader.GetValue(15).ToString()),
                                ItemSide = reader.GetValue(8).ToString(),
                                ItemSubType = reader.GetValue(5).ToString(),

                                Reference1 = reader.GetValue(10).ToString(),
                                Reference2 = reader.GetValue(11).ToString(),
                                Reference3 = reader.GetValue(12).ToString(),
                                TheyBalance = reader.GetValue(7).ToString().Contains("(") ? Convert.ToDouble(reader.GetValue(7)?.ToString().Trim('(', ')')) * -1 : Convert.ToDouble(reader.GetValue(7)?.ToString().Trim('(', ')')),
                                TransNarrative = reader.GetValue(9).ToString(),
                                WeBalance = reader.GetValue(6).ToString().Contains("(") ? Convert.ToDouble(reader.GetValue(7)?.ToString().Trim('(', ')')) * -1 : Convert.ToDouble(reader.GetValue(7)?.ToString().Trim('(', ')')),
                            };

                            openItems.Add(openItem);
                        }
                    }
                }
            }

            var olderThan3days = openItems.Where(i => i.DaysOverdue >= 3);
            var olderThan5days = openItems.Where(i => i.DaysOverdue >= 5);
            var olderThan7days = openItems.Where(i => i.DaysOverdue >= 7);
            var olderThan30days = openItems.Where(i => i.DaysOverdue >= 30);

            daysRecordsPairs.Add(3, olderThan3days.ToList());
            daysRecordsPairs.Add(5, olderThan5days.ToList());
            daysRecordsPairs.Add(7, olderThan7days.ToList());
            daysRecordsPairs.Add(30, olderThan30days.ToList());

            return CreateCsvFile(daysRecordsPairs);
        }

        private Dictionary<int, string> CreateCsvFile(Dictionary<int, List<OpenItem>> items)
        {
            var dict = new Dictionary<int, string>();

            foreach (var group in items) {

                var tempFilePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy_MM_dd_") + group.Key.ToString() + ".csv");

                using (var writer = new StreamWriter(tempFilePath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(group.Value);
                    }
                }

                dict.Add(group.Key, tempFilePath);
            }

            return dict;
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

    public class OpenItem
    {
        public string Account { get; set; }
        public string Entity { get; set; }
        public string AccName { get; set; }
        public DateTime PostedDate { get; set; }
        public double DaysOverdue { get; set; }
        public double Amount { get; set; }
        public string ItemSubType { get; set; }
        public double WeBalance { get; set; }
        public double TheyBalance { get; set; }
        public string ItemSide { get; set; }
        public string TransNarrative { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Reference3 { get; set; }
        public string FunctionalArea { get; set; }
        public string ActiveCertStatus { get; set; }
        public int ItemId { get; set; }
    }
}
