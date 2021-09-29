using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting
{
    public partial class ScheduledReporterJob : ConverterJobBase<ScheduledReporterJob>, IHostedService
    {
        public ScheduledReporterJob(ILogger<ScheduledReporterJob> logger, EmailSender emailSender,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _emailSender = emailSender;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Scheduled reporter job...");

            _semaphore = new SemaphoreSlim(1, 1);

            _timer = new Timer(async state => await ProcessNewReports(), null,
                TimeSpan.FromSeconds(new Random().Next(30, 60)), TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        /// <summary>
        /// This is the main method that does everything
        /// </summary>
        private async Task ProcessNewReports()
        {
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Running reporting job...");

                var config = GetDowloadConfiguration(_serviceScopeFactory);

                _logger.LogInformation($"Fetching tokens for {config.UserNamesAndPasswords.Count} users");

                var tokens = await GetDownloadLoginTokens(config, _logger);

                _logger.LogInformation($"Successfully fetched report tokens for {tokens.Count} users");

                foreach (var token in tokens)
                {
                    var allReports = (await GetRecentReports(config, token, _logger)).ToList();

                    _logger.LogInformation($"Fetched {allReports.Count} reports for user {token}");

                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                        var emailGroups = dbContext.EmailGroups.Where(g => g.IsActive).ToList();

                        foreach (var report in allReports)
                        {
                            if (dbContext.ProcessedReports.Any(r => r.ReportId == report.ReportId))
                                continue;

                            _logger.LogInformation($"Processing report {report.Name} with ID {report.ReportId}");

                            var reportPath = Path.Combine(await FileHelpers.GetTempPath(_serviceScopeFactory),
                                $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{report.Name}." +
                                (config.ExportType == "Excel" ? "xlsx" : config.ExportType));

                            var entity = dbContext.Configurations.FirstOrDefault(c =>
                                c.ConfigType == ConfigurationType.Setting && c.Key == "Entity").Value;

                            //if category does not exist set it to default and ignore it in the select filters after
                            SetReportFilters(report, entity);

                            var daysRange = GetEmailGroupDays(emailGroups, report.Country, report.Sprint,
                                report.Category);

                            await DownloadReportAndSendEmails(config, token, dbContext, report, reportPath, daysRange);
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

        private async Task DownloadReportAndSendEmails(ReportConfigModel config, string token,
            ApplicationDbContext dbContext, ReportModel report, string reportPath, int[] daysRange)
        {
            try
            {
                if (await DownloadReport(report.ReportId, config, reportPath, token, _logger))
                {
                    var results = await ProcessReportFile(reportPath, daysRange);

                    _logger.LogInformation($"Sending emails for report {report.Name} with ID {report.ReportId}");

                    if (results.Item2.Count > 0)
                    {
                        foreach (var key in results.Item2)
                        {
                            //key is the overdue days used to select the email groups
                            var emails = GetEmails(key.Key, report.Country, report.Sprint, report.Category,
                                _serviceScopeFactory);

                            //ONLY SEND EMAILS IF FILE HAS 1 OR MORE RECORDS
                            await _emailSender.SendMessage(emails,
                                config.EmailHeader + $" Report ID: {report.ReportId}",
                                config.EmailBody + Environment.NewLine + $"{key.Key} Days overdue" +
                                Environment.NewLine +
                                $"Report Name {report.Name}" + Environment.NewLine +
                                //$"Report for {country} for {sprint}" + Environment.NewLine +
                                $"Report generated by: {report.Creator}" + Environment.NewLine +
                                $"COMMENTS:- {report.Notes}", false,
                                new[] { results.Item1, key.Value });

                            await Task.Delay(7000);
                        }
                    }
                    else
                    {
                        foreach (var r in daysRange)
                        {
                            var outputFile = results.Item1;

                            //change signage for Tz B.P. report
                            if (report.Name.ToLower().Contains("tanzania") && report.Name.ToLower().Contains("clearing")
                                                                           && report.Name.ToLower()
                                                                               .Contains("suspense") &&
                                                                           report.Name.ToLower().Contains("proofing"))
                                outputFile = await AdjustBalanceValue(results.Item1);

                            await _emailSender.SendMessage(GetEmails(r, report.Country, report.Sprint, report.Category, _serviceScopeFactory),
                                config.EmailHeader,
                                config.EmailBody + Environment.NewLine +
                                $"Report Name {report.Name}" + Environment.NewLine +
                                //$"Report for {country} for {sprint}" + Environment.NewLine +
                                $"Report generated by: {report.Creator}" + Environment.NewLine +
                                $"COMMENTS:- {report.Notes}",
                                filePaths: new[] { outputFile });
                        }

                        await Task.Delay(7000);
                    }

                    await SaveToDb(report, dbContext, config);

                    _logger.LogInformation($"Finished processing report {report.Name} with ID {report.ReportId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// Set the report filters so that we can know which emails to send to
        /// </summary>
        private static void SetReportFilters(ReportModel report, string entity)
        {
            var country = Country.Kenya;
            var sprint = Sprint.Nostro;
            var category = ReportCategory.Default;

            if (entity == "IMTZ") country = Country.Tanzania;
            if (entity == "IMRW") country = Country.Rwanda;

            //SET COUNTRY
            //Kenya
            if (report.Name.ToLower().Contains("kenya")) country = Country.Kenya;
            //Rwanda
            if (report.Name.ToLower().Contains("rwanda")) country = Country.Rwanda;
            //Tanzania
            if (report.Name.ToLower().Contains("tanzania")) country = Country.Tanzania;

            //SET SPRINT

            //Nostros
            if (report.Name.ToLower().Contains("nostro")) sprint = Sprint.Nostro;
            //Mobile banking
            if (report.Name.ToLower().Contains("mb")) sprint = Sprint.Mobile_Banking;
            //Cards
            if (report.Name.ToLower().Contains("cards")) sprint = Sprint.Cards;
            //Suspense
            if (report.Name.ToLower().Contains("suspense")) sprint = Sprint.Suspense;
            //others
            if (report.Name.ToLower().Contains("abc")) sprint = Sprint.ABC;

            //SET CATEGORY

            foreach (int val in Enum.GetValues(typeof(ReportCategory)))
            {
                var checkVals = EnumHelpers.GetDescriptors((ReportCategory)val);

                if (checkVals.All(x => report.Name.ToLower().Contains(x.ToLower()))) category = (ReportCategory)val;
            }

            report.Category = category;
            report.Sprint = sprint;
            report.Country = country;
        }

        /// <summary>
        /// Save the report if successfully processed to avoid resending it in the future
        /// </summary>
        private async Task SaveToDb(ReportModel report, ApplicationDbContext dbContext, ReportConfigModel config)
        {
            dbContext.ProcessedReports.Add(new ProcessedReport
            {
                Format = config.ExportType,
                ReportId = report.ReportId,
                Name = report.Name,
                ProcessedDate = DateTime.Now,
                Creator = report.Creator,
                EndTime = report.EndTime,
                StartTime = report.StartTime,
                Message = report.Message,
                Notes = report.Notes,
                Status = report.Status,
                UserToken = report.UserToken
            });

            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Process the content in the downloaded report file including calculating days overdue
        /// </summary>
        /// <param name="savedFile"></param>
        /// <returns>List of key: email group name and value: list of files to send to them</returns>
        private async Task<(string, Dictionary<int, string>)> ProcessReportFile(string inputFile, int[] daysRange)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var daysRecordsPairs = new Dictionary<int, List<OpenItem>>();

            var openItems = new List<OpenItem>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var lastAccountNo = string.Empty;

                    while (reader.Read())
                    {
                        var col3 = reader.GetValue(3)?.ToString();
                        if (string.IsNullOrEmpty(col3)) continue;

                        DateTime postedDate;

                        if (DateTime.TryParse(col3, out postedDate))
                            try
                            {
                                var daysOverdue =
                                    Convert.ToInt32((DateTime.Now - postedDate)
                                        .TotalDays); //datetime.now should be max posted date

                                var openItem = new OpenItem
                                {
                                    DaysOverdue = daysOverdue,
                                    PostedDate = postedDate,
                                    AccName = reader.GetValue(2)?.ToString(),
                                    //Account = lastAccountNo,
                                    Amount = reader.GetValue(4)?.ToString(),
                                    Entity = reader.GetValue(1)?.ToString(),
                                    //ActiveCertStatus = reader.GetValue(14)?.ToString(),
                                    //FunctionalArea = reader.GetValue(13)?.ToString(),
                                    //ItemId = Convert.ToInt32(reader.GetValue(15)?.ToString()),
                                    ItemSide = reader.GetValue(8)?.ToString(),
                                    ItemSubType = reader.GetValue(5)?.ToString(),

                                    Reference1 = reader.GetValue(10)?.ToString(),
                                    Reference2 = reader.GetValue(11)?.ToString(),
                                    Reference3 = reader.GetValue(12)?.ToString(),
                                    TheyBalance = reader.GetValue(7)?.ToString(),
                                    TransNarrative = reader.GetValue(9)?.ToString(),
                                    WeBalance = reader.GetValue(6)?.ToString()
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

            for (var i = 0; i < daysRange.Length; i++)
            {
                List<OpenItem> items;

                if (i + 1 < daysRange.Length)
                    items = openItems.Where(it => it.DaysOverdue >= daysRange[i] && it.DaysOverdue < daysRange[i + 1])
                        .ToList();
                else
                    items = openItems.Where(it => it.DaysOverdue >= daysRange[i]).ToList();

                daysRecordsPairs.Add(daysRange[i], items);
            }

            var agingExcel = await CreateModifiedAgingExcel(inputFile, daysRange);

            if (daysRecordsPairs.Any())
                return (agingExcel, await CreateCsvFile(daysRecordsPairs, _serviceScopeFactory));
            return (inputFile, new Dictionary<int, string>());
        }

        private async Task<string> CreateModifiedAgingExcel(string inputFile, int[] daysRange)
        {
            var inputFileName = Path.GetFileName(inputFile);

            var outputFilePath =
                Path.Combine(await FileHelpers.GetTempPath(_serviceScopeFactory), "Aged_" + inputFileName);

            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                var sheet = package.Workbook.Worksheets.First();

                var maxDateInt = 0;

                var maxDate = DateTime.Now;

                try
                {
                    maxDateInt = sheet.Cells["D:D"].Max(c =>
                    {
                        if (int.TryParse(c.Value?.ToString(), out var result)) return result;

                        return 0;
                    });

                    maxDate = FromExcelSerialDate(maxDateInt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error obtaining excel date");
                }

                sheet.InsertColumn(5, 1);

                //set maxDate only if it is not a balance proofing report
                if (!inputFileName.ToLower().Contains("proofing"))
                {
                    sheet.Cells["A5"].Value = $"Recon Date: {maxDate:MM/dd/yyyy}";
                }

                sheet.Cells["A5"].Style.Font.Bold = true;
                //set header
                sheet.Cells["E6"].Value = "DAYS OVERDUE";

                //set formula for cells
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                for (var i = start.Row + 7; i <= end.Row; i++)
                {
                    var dateFromExcel = sheet.Cells[$"D{i}"].Value?.ToString();

                    if (dateFromExcel != null && int.TryParse(dateFromExcel, out var dateInt))
                    {
                        var outputDate = FromExcelSerialDate(dateInt);

                        var diff = (maxDate - outputDate).Days;

                        sheet.Cells[$"E{i}"].Formula =
                            $"=IF(NOT(ISBLANK(D{i})),DATEDIF(D{i}, {maxDateInt}, \"D\"),\"\")";

                        sheet.Cells[$"E{i}"].Style.Numberformat.Format = "0";


                        if (daysRange.Length >= 2 && diff >= daysRange[0] && diff <= daysRange[1])
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.GreenYellow);

                        if (daysRange.Length >= 3 && diff > daysRange[1] && diff <= daysRange[2])
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.RosyBrown);

                        if (daysRange.Length >= 4 && diff > daysRange[2] && diff <= daysRange[3])
                            sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.Yellow);

                        if (diff > 30) sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.Red);
                    }
                }

                //save new excel
                await package.SaveAsAsync(new FileInfo(outputFilePath));
            }

            return outputFilePath;
        }
    }
}