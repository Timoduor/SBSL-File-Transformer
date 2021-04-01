using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class ScheduledReporterJob : IHostedService
    {
        private readonly ILogger<ScheduledReporterJob> _logger;
        private readonly EmailSender _emailSender;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;
        private static SemaphoreSlim _semaphore;

        public ScheduledReporterJob(ILogger<ScheduledReporterJob> logger, EmailSender emailSender, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _emailSender = emailSender;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduled reporter job...");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async (state) => await ProcessNewReport(), null, TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private async Task ProcessNewReport()
        {

            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running reporting job...");

                var config = GetConfiguration();

                //FOR TEST PURPOSES ONLY
                //{
                //    var testResults = ProcessReportFile(@"C:\Users\Yida\Downloads\KE Nostro Open Items Daily Report.xlsx");

                //    foreach (var key in testResults.Item2)
                //    {
                //        //key is the overdue days used to select the email groups
                //        var emails = GetEmails(key.Key);

                //        await _emailSender.SendMessage(emails, $"Overdue recons by {key.Key} days or more", $"This is an auto-generated report for reconciliations overdue by {key.Key} days or more", filePaths: new string[] { testResults.Item1, key.Value });
                //    }
                //}
                _logger.LogInformation($"Fetching tokens for {config.UserNamesAndPasswords.Count} users");

                var tokens = await GetLoginTokens(config);

                _logger.LogInformation($"Successfully fetched report tokens for {tokens.Count} users");

                foreach (var token in tokens)
                {

                    var allReports = (await GetRecentReports(config, token)).ToList();

                    _logger.LogInformation($"Fetched {allReports.Count} reports for user {token}");

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        foreach (var report in allReports)
                        {
                            if (dbContext.ProcessedReports.Any(r => r.ReportId == report.ReportId))
                            {
                                continue;
                            }

                            _logger.LogInformation($"Processing report {report.Name} with ID {report.ReportId}");

                            var reportPath = Path.Combine(await FileHelpers.GetTempPath(_serviceScopeFactory),
                                $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{report.Name}." +
                                (config.ExportType == "Excel" ? "xlsx" : config.ExportType));


                            //send only to kenyan users
                            if (report.Name.ToLower().Contains("kenya"))
                            {
                                //send it to kenyan emails
                            }
                            if (report.Name.ToLower().Contains("rwanda"))
                            {

                            }
                            if (report.Name.ToLower().Contains("tanzania"))
                            {

                            }


                            try
                            {

                                if (await DownloadReport(report.ReportId, config, reportPath, token))
                                {
                                    var results = await ProcessReportFile(reportPath);

                                    _logger.LogInformation($"Sending emails for report {report.Name} with ID {report.ReportId}");

                                    if (results.Item2.Count > 0)
                                    {
                                        foreach (var key in results.Item2)
                                        {
                                            //key is the overdue days used to select the email groups
                                            var emails = GetEmails(key.Key);

                                            //ONLY SEND EMAILS IF FILE HAS 1 OR MORE RECORDS
                                            await _emailSender.SendMessage(emails, config.EmailHeader + $" Report ID: { report.ReportId }",
                                                config.EmailBody + Environment.NewLine + $"{ key.Key } Days overdue", false,
                                                filePaths: new string[] { results.Item1, key.Value });

                                            await Task.Delay(7000);
                                        }
                                    }
                                    else
                                    {
                                        await _emailSender.SendMessage(GetEmails(3), config.EmailHeader,
                                            config.EmailBody, filePaths: new string[] { results.Item1 });

                                        await Task.Delay(7000);
                                    }

                                    await SaveToDb(report, dbContext, config);

                                    _logger.LogInformation($"Finished processing report {report.Name} with ID {report.ReportId}");
                                }
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);
                            }
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
                _semaphore.Release();
            }
        }

        private async Task SaveToDb(ReportModel report, ApplicationDbContext dbContext, ReportConfigModel config)
        {
            dbContext.ProcessedReports.Add(new ProcessedReport
            {
                Format = config.ExportType,
                ReportId = report.ReportId,
                Name = report.Name,
                ProcessedDate = DateTime.Now
            });

            await dbContext.SaveChangesAsync();
        }

        private async Task<List<string>> GetLoginTokens(ReportConfigModel config)
        {
            var tokens = new List<string>();

            try
            {
                foreach (var user in config.UserNamesAndPasswords)
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(10);

                        var content = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("scope", config.Scope),
                            new KeyValuePair<string, string>("username", user.Key),
                            new KeyValuePair<string, string>("password", user.Value),
                            new KeyValuePair<string, string>("client_id", config.ClientId),
                            new KeyValuePair<string, string>("client_secret", config.ClientSecret),
                        };

                        var formdata = new FormUrlEncodedContent(content);

                        var response = await client.PostAsync(config.TokenUrl, formdata);

                        if (response.IsSuccessStatusCode)
                        {
                            var respContent = await response.Content.ReadAsStringAsync();

                            var data = JObject.Parse(respContent);

                            tokens.Add((string)data.SelectToken("access_token"));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return tokens;
        }

        private async Task<IEnumerable<ReportModel>> GetRecentReports(ReportConfigModel config, string token)
        {
            var reportsUrl = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/queryruns";

            var reports = new List<ReportModel>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

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
                                Status = item.status,
                                UserToken = token
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

                var groups = dbContext.EmailGroups.Where(g => g.AgeAlertDuration == key && g.IsActive);

                var groupEmails = groups.ToList().Select(g => g.Emails);

                foreach (var group in groupEmails)
                {
                    emails.AddRange(group.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                }

                return emails;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="savedFile"></param>
        /// <returns>List of key: email group name and value: list of files to send to them</returns>
        private async Task<(string, Dictionary<int, string>)> ProcessReportFile(string inputFile)
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

                        if (DateTime.TryParse(col3, out postedDate))
                        {
                            try
                            {
                                int daysOverdue = Convert.ToInt32((DateTime.Now - postedDate).TotalDays);//datetime.now should be max posted date

                                var openItem = new OpenItem
                                {
                                    DaysOverdue = daysOverdue,
                                    PostedDate = postedDate,
                                    AccName = reader.GetValue(2)?.ToString(),
                                    //Account = lastAccountNo,
                                    Amount = reader.GetValue(4)?.ToString(),
                                    Entity = reader.GetValue(1)?.ToString(),
                                    ActiveCertStatus = reader.GetValue(14)?.ToString(),
                                    FunctionalArea = reader.GetValue(13)?.ToString(),
                                    //ItemId = Convert.ToInt32(reader.GetValue(15)?.ToString()),
                                    ItemSide = reader.GetValue(8)?.ToString(),
                                    ItemSubType = reader.GetValue(5)?.ToString(),

                                    Reference1 = reader.GetValue(10)?.ToString(),
                                    Reference2 = reader.GetValue(11)?.ToString(),
                                    Reference3 = reader.GetValue(12)?.ToString(),
                                    TheyBalance = reader.GetValue(7)?.ToString(),
                                    TransNarrative = reader.GetValue(9)?.ToString(),
                                    WeBalance = reader.GetValue(6)?.ToString(),
                                };

                                openItems.Add(openItem);
                            }
                            catch (Exception ex)
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

            if (olderThan3days.Any())
            {
                daysRecordsPairs.Add(3, olderThan3days.ToList());
            }
            if (olderThan5days.Any())
            {
                daysRecordsPairs.Add(5, olderThan5days.ToList());
            }
            if (olderThan7days.Any())
            {
                daysRecordsPairs.Add(7, olderThan7days.ToList());
            }
            if (olderThan30days.Any())
            {
                daysRecordsPairs.Add(30, olderThan30days.ToList());
            }

            var agingExcel = await CreateModifiedAgingExcel(inputFile);

            if (daysRecordsPairs.Any())
            {
                return (agingExcel, await CreateCsvFile(daysRecordsPairs));
            }
            else
            {
                return (inputFile, new Dictionary<int, string>());
            }
        }

        private async Task<string> CreateModifiedAgingExcel(string inputFile)
        {
            var inputFileName = Path.GetFileName(inputFile);

            var outputFilePath = Path.Combine(await FileHelpers.GetTempPath(_serviceScopeFactory), "Aged_" + inputFileName);

            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                var sheet = package.Workbook.Worksheets.First();

                var maxDateInt = sheet.Cells["D:D"].Max(c =>
                {
                    if (int.TryParse(c.Value?.ToString(), out int result))
                    {
                        return result;
                    }

                    return 0;
                });

                var maxDate = FromExcelSerialDate(maxDateInt);

                sheet.InsertColumn(5, 1);

                //set maxDate
                sheet.Cells["A5"].Value = $"Recon Date: {maxDate:MM/dd/yyyy}";
                sheet.Cells["A5"].Style.Font.Bold = true;
                //set header
                sheet.Cells["E6"].Value = "DAYS OVERDUE";

                //set formula for cells
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                for (var i = start.Row + 7; i <= end.Row; i++)
                {
                    var dateFromExcel = sheet.Cells[$"D{i}"].Value?.ToString();

                    if (dateFromExcel != null && int.TryParse(dateFromExcel, out int dateInt))
                    {
                        var outputDate = FromExcelSerialDate(dateInt);

                        var diff = (maxDate - outputDate).Days;

                        sheet.Cells[$"E{i}"].Formula =
                            $"=IF(NOT(ISBLANK(D{i})),DATEDIF(D{i}, {maxDateInt}, \"D\"),\"\")";

                        sheet.Cells[$"E{i}"].Style.Numberformat.Format = "0";


                        if (diff >= 3 && diff <= 5)
                        {
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.GreenYellow);
                        }

                        if (diff > 5 && diff <= 7)
                        {
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.RosyBrown);
                        }

                        if (diff > 7 && diff <= 30)
                        {
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.Yellow);
                        }

                        if (diff > 30)
                        {
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.Red);
                        }

                    }

                }

                //save new excel
                await package.SaveAsAsync(new FileInfo(outputFilePath));
            }

            return outputFilePath;
        }

        private DateTime FromExcelSerialDate(int SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug
            return new DateTime(1899, 12, 31).AddDays(SerialDate);
        }

        private async Task<Dictionary<int, string>> CreateCsvFile(Dictionary<int, List<OpenItem>> items)
        {
            var dict = new Dictionary<int, string>();

            foreach (var group in items)
            {

                var tempFilePath = Path.Combine(await FileHelpers.GetTempPath(_serviceScopeFactory), DateTime.Now.ToString("yyyy_MM_dd_") + group.Key + "_Days_Overdue_.csv");

                using (var writer = new StreamWriter(tempFilePath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        await csv.WriteRecordsAsync(@group.Value);
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

                var userLogins = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.ReportUser).ToList();

                var config = new ReportConfigModel
                {
                    BaseUrl = configurations.FirstOrDefault(c => c.Key == "BaseUrl")?.Value,
                    EnvironmentUrl = configurations.FirstOrDefault(c => c.Key == "EnvironmentUrl")?.Value,
                    UserToken = configurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                    EmailBody = configurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                    EmailHeader = configurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                    ExportType = configurations.FirstOrDefault(c => c.Key == "ExportType")?.Value,
                    Scope = configurations.FirstOrDefault(c => c.Key == "Scope")?.Value,
                    TokenUrl = configurations.FirstOrDefault(c => c.Key == "TokenUrl")?.Value,
                    ClientId = configurations.FirstOrDefault(c => c.Key == "ClientId")?.Value,
                    ClientSecret = configurations.FirstOrDefault(c => c.Key == "ClientSecret")?.Value,
                };

                config.UserNamesAndPasswords = new Dictionary<string, string>();

                foreach (var login in userLogins)
                {
                    config.UserNamesAndPasswords.Add(login.Key, login.Value);
                }

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
                    client.Timeout = TimeSpan.FromMinutes(10);

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
        public string ItemSubType { get; set; }
        public string WeBalance { get; set; }
        public string TheyBalance { get; set; }
        public string ItemSide { get; set; }
        public string TransNarrative { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Reference3 { get; set; }
        public string FunctionalArea { get; set; }
        public string ActiveCertStatus { get; set; }
        public string ItemId { get; set; }
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
        public string UserToken { get; set; }
    }
}
