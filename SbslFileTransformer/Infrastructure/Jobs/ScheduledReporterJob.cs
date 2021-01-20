using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class ScheduledReporterJob : IHostedService
    {
        private readonly ILogger<ScheduledReporterJob> _logger;
        private readonly EmailSender _emailSender;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;
        volatile bool _isRunning;

        public ScheduledReporterJob(ILogger<ScheduledReporterJob> logger, EmailSender emailSender, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _emailSender = emailSender;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduled reporter job...");

            _timer = new Timer(async (state) => await ProcessNewReport(), null, TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromHours(12));

            return Task.CompletedTask;
        }

        private async Task ProcessNewReport()
        {
            try
            {
                if (_isRunning)
                {
                    return;
                }

                _isRunning = true;

                _logger.LogInformation("Running reporting job");

                var config = GetConfiguration();

                //FOR TEST PURPOSES ONLY
                //{
                //    var testResults = ProcessReportFile(@"C:\Users\Yida\Downloads\CBK Open Items Daily Report (8).xlsx");

                //    foreach (var key in testResults)
                //    {
                //        //key is the overdue days used to select the email groups
                //        var emails = GetEmails(key.Key);

                //        await _emailSender.SendMessage(emails, $"Overdue recons by {key.Key} days or more", $"This is an auto-generated report for reconciliations overdue by {key.Key} days or more", filePaths: new string[] { key.Value });
                //    }
                //}


                var token = await GetLoginToken(config);

                var allReports = await GetRecentReports(config, token);

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    foreach (var report in allReports)
                    {
                        if (dbContext.ProcessedReports.Any(r => r.ReportId == report.ReportId))
                        {
                            continue;
                        }

                        var reportPath = Path.Combine(Path.GetTempPath(), $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{report.Name}." + (config.ExportType == "Excel" ? "xlsx" : config.ExportType));

                        if (await DownloadReport(report.ReportId, config, reportPath, token))
                        {
                            var results = ProcessReportFile(reportPath);

                            foreach (var key in results.Item2)
                            {
                                //key is the overdue days used to select the email groups
                                var emails = GetEmails(key.Key);

                                //ONLY SEND EMAILS IF FILE HAS 1 OR MORE RECORDS

                                await _emailSender.SendMessage(emails, config.EmailHeader, config.EmailBody, filePaths: new string[] { results.Item1, key.Value });
                            }
                            await SaveToDb(report, dbContext, config);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task SaveToDb(ReportModel report, ApplicationDbContext dbContext, ReportConfigModel config)
        {
            dbContext.ProcessedReports.Add(new ProcessedReport {
                Format = config.ExportType,
                ReportId = report.ReportId,
                Name = report.Name,
                ProcessedDate = DateTime.Now
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task<string> GetLoginToken(ReportConfigModel config)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>( "grant_type", "password" ),
                        new KeyValuePair<string, string>( "scope", config.Scope ),
                        new KeyValuePair<string, string>( "username", config.UserName ),
                        new KeyValuePair<string, string>( "client_id", config.ClientId ),
                        new KeyValuePair<string, string>( "client_secret", config.ClientSecret ),
                        new KeyValuePair<string, string>( "password", config.Password ),
                    };

                    var formdata = new FormUrlEncodedContent(content);

                    var response = await client.PostAsync(config.TokenUrl, formdata);

                    if (response.IsSuccessStatusCode)
                    {
                        var respContent = await response.Content.ReadAsStringAsync();

                        dynamic data = JObject.Parse(respContent);

                        return data.access_token;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return string.Empty;
        }

        private async Task<IEnumerable<ReportModel>> GetRecentReports(ReportConfigModel config, string token)
        {
            var reportsUrl = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/queryruns";

            var reports = new List<ReportModel>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(reportsUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        dynamic data = JArray.Parse(result);

                        foreach (var item in data)
                        {
                            reports.Add(new ReportModel
                            {
                                Creator = item.creatorFirstAndLastName,
                                EndTime = item.endTime,
                                Message = item.endTime,
                                Name = item.name,
                                Notes = item.notes,
                                ReportId = item.id,
                                StartTime = item.startTime,
                                Status = item.status
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return reports;
        }

        private IEnumerable<string> GetEmails(int key)
        {
            var emails = new List<string>();

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var groups = dbContext.EmailGroups.Where(g => g.AgeAlertDuration >= key && g.IsActive);

                var groupEmails = groups.ToList().Select(g => g.Emails);

                foreach(var group in groupEmails)
                {
                    emails.AddRange(group.Split(',', '\r', '\n'));
                }

                return emails;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="savedFile"></param>
        /// <returns>List of key: email group name and value: list of files to send to them</returns>
        private (string, Dictionary<int, string>) ProcessReportFile(string inputFile)
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
                            try
                            {
                                var openItem = new OpenItem
                                {
                                    DaysOverdue = Convert.ToInt32((DateTime.Now - postedDate).TotalDays),
                                    PostedDate = postedDate,
                                    AccName = reader.GetValue(2)?.ToString(),
                                    //Account = lastAccountNo,
                                    Amount = reader.GetValue(4)?.ToString(),
                                    Entity = reader.GetValue(1)?.ToString(),
                                    //ActiveCertStatus = reader.GetValue(14)?.ToString(),
                                    //FunctionalArea = reader.GetValue(13)?.ToString(),
                                    //ItemId = Convert.ToInt32(reader.GetValue(15)?.ToString()),
                                    //ItemSide = reader.GetValue(8)?.ToString(),
                                    //ItemSubType = reader.GetValue(5)?.ToString(),

                                    //Reference1 = reader.GetValue(10)?.ToString(),
                                    //Reference2 = reader.GetValue(11)?.ToString(),
                                    //Reference3 = reader.GetValue(12)?.ToString(),
                                    //TheyBalance = reader.GetValue(7)?.ToString(),
                                    //TransNarrative = reader.GetValue(9)?.ToString(),
                                    //WeBalance = reader.GetValue(6)?.ToString(),
                                };

                                openItems.Add(openItem);
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);
                            }
                        }
                    }
                }
            }

            var olderThan3days = openItems.Where(i => i.DaysOverdue >= 3 && i.DaysOverdue < 5);
            var olderThan5days = openItems.Where(i => i.DaysOverdue >= 5 && i.DaysOverdue < 7);
            var olderThan7days = openItems.Where(i => i.DaysOverdue >= 7 && i.DaysOverdue < 30);
            var olderThan30days = openItems.Where(i => i.DaysOverdue >= 30);

            if (olderThan3days.Count() > 0)
            {
                daysRecordsPairs.Add(3, olderThan3days.ToList());
            }
            if (olderThan5days.Count() > 0)
            {
                daysRecordsPairs.Add(5, olderThan5days.ToList());
            }
            if (olderThan7days.Count() > 0)
            {
                daysRecordsPairs.Add(7, olderThan7days.ToList());
            }
            if (olderThan7days.Count() > 0)
            {
                daysRecordsPairs.Add(30, olderThan30days.ToList());
            }

            var agingExcel = CreateModifiedAgingExcel(inputFile);

            if (daysRecordsPairs.Count() > 0)
            {
                return (agingExcel, CreateCsvFile(daysRecordsPairs));
            }
            else
            {
                return (inputFile,  new Dictionary<int, string>());
            }
        }

        private string CreateModifiedAgingExcel(string inputFile)
        {
            var inputFileName = Path.GetFileName(inputFile);

            var outputFilePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy_MM_dd_") + inputFileName);

            var app = new Application();
            var workbook = app.Workbooks.Open(inputFile);
            var sheet = (Worksheet)workbook.Worksheets[1];

            Range range = sheet.Range["E1"];

            range.EntireColumn.Insert(XlInsertShiftDirection.xlShiftToRight,
                XlInsertFormatOrigin.xlFormatFromRightOrBelow);

            //set header
            sheet.Range["E6"].Value = "Days Overdue";

            //set formula for cells
            var rows = sheet.UsedRange.Rows.Count;

            for(var i = 7; i <= rows; i++)
            {
                DateTime outputDate;

                var dateFromExcel = sheet.Range[$"D{i}"].Value?.ToString();

                if (dateFromExcel != null && DateTime.TryParse(dateFromExcel, out outputDate))
                {
                    //var diff = (DateTime.Now - outputDate).Days;

                    sheet.Range[$"E{i}"].Formula = $"=IF(NOT(ISBLANK(D{i})),DATEDIF(D{i}, TODAY(), \"D\"),\"\")";

                    sheet.Range[$"E{i}"].NumberFormat = "0";

                    int diff = 0;

                    if (Int32.TryParse(sheet.Range[$"E{i}"].Value.ToString(), out diff))
                    {
                        if (diff >= 3 && diff <= 5)
                        {
                            sheet.Range[$"E{i}"].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.GreenYellow);
                        }
                        if (diff > 5 && diff <= 7)
                        {
                            sheet.Range[$"E{i}"].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.RosyBrown);
                        }
                        if (diff > 7 && diff <= 30)
                        {
                            sheet.Range[$"E{i}"].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Yellow);
                        }
                        if (diff > 30)
                        {
                            sheet.Range[$"E{i}"].Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Red);
                        }
                    }
                }
            }

            //save new excel
            app.DisplayAlerts = false;
            workbook.SaveAs2(outputFilePath);
            workbook.Close(true);

            return outputFilePath;
        }

        private Dictionary<int, string> CreateCsvFile(Dictionary<int, List<OpenItem>> items)
        {
            var dict = new Dictionary<int, string>();

            foreach (var group in items)
            {

                var tempFilePath = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyy_MM_dd_") + group.Key + ".csv");

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
                    Password = configurations.FirstOrDefault(c => c.Key == "Password")?.Value,
                    UserToken = configurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                    EmailBody = configurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                    EmailHeader = configurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                    ExportType = configurations.FirstOrDefault(c => c.Key == "ExportType")?.Value,
                    Scope = configurations.FirstOrDefault(c => c.Key == "Scope")?.Value,
                    TokenUrl = configurations.FirstOrDefault(c => c.Key == "TokenUrl")?.Value,
                    ClientId = configurations.FirstOrDefault(c => c.Key == "ClientId")?.Value,
                    ClientSecret = configurations.FirstOrDefault(c => c.Key == "ClientSecret")?.Value,
                };

                return config;
            }
        }

        private async Task<bool> DownloadReport(long reportId, ReportConfigModel config, string savePath, string token)
        {
            var reportToDownload = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/completedqueryrun/{reportId}/{config.ExportType}";
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(reportToDownload);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStreamAsync();

                        using (var fs = File.Create(savePath))
                        {
                            result.Seek(0, SeekOrigin.Begin);
                            result.CopyTo(fs);
                        }

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return false;
        }
    }

    public class OpenItem
    {
        //public string Account { get; set; }
        public string Entity { get; set; }
        public string AccName { get; set; }
        public DateTime PostedDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Amount { get; set; }
        //public string ItemSubType { get; set; }
        //public string WeBalance { get; set; }
        //public string TheyBalance { get; set; }
        //public string ItemSide { get; set; }
        //public string TransNarrative { get; set; }
        //public string Reference1 { get; set; }
        //public string Reference2 { get; set; }
        //public string Reference3 { get; set; }
        //public string FunctionalArea { get; set; }
        //public string ActiveCertStatus { get; set; }
        //public string ItemId { get; set; }
    }

    public class ReportModel
    {
        public string Creator { get; set; }
        public string EndTime { get; set; }
        public long ReportId { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
    }
}
