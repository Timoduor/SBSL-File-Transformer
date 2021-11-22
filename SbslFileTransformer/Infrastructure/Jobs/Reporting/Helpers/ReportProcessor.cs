using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers
{
    public class ReportProcessor
    {
        readonly ILogger<ReportEngineJob> Logger;
        readonly IServiceScopeFactory ServiceScopeFactory;
        readonly ReportConfigModel ReportConfigModel;

        public ReportProcessor(ILogger<ReportEngineJob> logger, IServiceScopeFactory serviceScopeFactory, ReportConfigModel reportConfigModel)
        {
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
            ReportConfigModel = reportConfigModel;
        }

        public async Task ProcessReports(Dictionary<string, IEnumerable<ReportModel>> unprocessedReports, string entity, IProgress<int> processReportProgress)
        {
            using (IServiceScope scope = ServiceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                List<EmailGroup> emailGroups = dbContext.EmailGroups.ToList();

                foreach (KeyValuePair<string, IEnumerable<ReportModel>> reportUser in unprocessedReports)
                {
                    Logger.LogInformation($"Processing reports for user {reportUser.Key}");

                    foreach (IEnumerable<ReportModel> reportBatch in reportUser.Value.Batch(25))
                    {
                        try
                        {
                            await ProcessReportBatch(reportBatch, entity, processReportProgress, emailGroups);
                        }
                        catch(Exception ex)
                        {
                            Logger.LogError(ex, $"Error processing report batch for user {reportUser.Key}");
                        }
                    }
                }
            }
        }

        private async Task ProcessReportBatch(IEnumerable<ReportModel> reports, string entity, IProgress<int> processReportProgress, List<EmailGroup> emailGroups)
        {
            //keyvalue pair of inputfile and dict of days/output csv files
            List<(ReportModel, Dictionary<int, string>)> processedReports = new List<(ReportModel, Dictionary<int, string>)>();

            int count = 0;

            foreach (ReportModel report in reports)
            {
                Logger.LogInformation($"Processing report {report.Name} with ID {report.ReportId}");

                SetReportFilters(report, entity);

                if (await DownloadAndSaveReport(report))
                {
                    report.DaysRange = GetEmailGroupDays(emailGroups, report);

                    processedReports.Add(await ProcessReportFile(report));
                }

                count++;

                processReportProgress.Report(count * 100 / reports.Count());
            }

            await SaveAndSendReports(processedReports, processReportProgress);
        }

        private async Task SaveAndSendReports(List<(ReportModel, Dictionary<int, string>)> processedReports, IProgress<int> processReportProgress)
        {
            ReportSender reportSender = new ReportSender(Logger, ServiceScopeFactory, ReportConfigModel);

            await reportSender.SendAndSaveReports(processedReports, processReportProgress);
        }

        private async Task<bool> DownloadAndSaveReport(ReportModel report)
        {
            Logger.LogInformation($"Downloading report ID: {report.ReportId} Title: {report.Name}");

            report.TempReportPath = Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory),
                                $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{report.Name}." +
                                (ReportConfigModel.ExportType == "Excel" ? "xlsx" : ReportConfigModel.ExportType));

            string reportToDownload =
                @$"https://{ReportConfigModel.EnvironmentUrl}.{ReportConfigModel.BaseUrl}/completedqueryrun/{report.ReportId}/{ReportConfigModel.ExportType}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", report.UserToken);

                    HttpResponseMessage response = await client.GetAsync(reportToDownload);

                    if (response.IsSuccessStatusCode)
                    {
                        Stream result = await response.Content.ReadAsStreamAsync();

                        using (FileStream fs = File.Create(report.TempReportPath))
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
                Logger.LogError(ex, ex.Message);
            }

            return false;
        }

        private int[] GetEmailGroupDays(List<EmailGroup> emailGroups, ReportModel report)
        {
            IEnumerable<EmailGroup> groups = emailGroups.Where(g => g.Country == report.Country && g.Sprint == report.Sprint && g.Category == report.Category);

            if (report.Category == ReportCategory.Default)
                groups = emailGroups.Where(g => g.Country == report.Country && g.Sprint == report.Sprint);

            int[] daysRange = groups.OrderBy(g => g.AgeAlertDuration).Select(g => g.AgeAlertDuration).ToArray();

            return daysRange;
        }

        private static void SetReportFilters(ReportModel report, string entity)
        {
            Country country = Country.Kenya;
            Sprint sprint = Sprint.Nostro;
            ReportCategory category = ReportCategory.Default;

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
            
            if (entity == "BOAKE")
            {
                //finance boa
                if (report.Name.ToLower().Contains("fin")) sprint = Sprint.Finance;
                //money transfers boa
                if (report.Name.ToLower().Contains("mt")) sprint = Sprint.MoneyTransfers;
                // bill payments boa
                if (report.Name.ToLower().Contains("bill")) sprint = Sprint.MoneyTransfers;
            }

            //SET CATEGORY
            foreach (int val in Enum.GetValues(typeof(ReportCategory)))
            {
                string[] checkVals = EnumHelpers.GetDescriptors((ReportCategory)val);

                if (checkVals.All(x => report.Name.ToLower().Contains(x.ToLower())))
                    category = (ReportCategory)val;
            }

            report.Category = category;
            report.Sprint = sprint;
            report.Country = country;
        }

        private async Task<(ReportModel, Dictionary<int, string>)> ProcessReportFile(ReportModel report)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Dictionary<int, List<OpenItem>> daysRecordsPairs = new Dictionary<int, List<OpenItem>>();

            List<OpenItem> openItems = new List<OpenItem>();

            using (FileStream stream = File.Open(report.TempReportPath, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    string lastAccountNo = string.Empty;

                    while (reader.Read())
                    {
                        string col3 = reader.GetValue(3)?.ToString();
                        if (string.IsNullOrEmpty(col3)) continue;

                        if (DateTime.TryParse(col3, out DateTime postedDate))
                            try
                            {
                                int daysOverdue =
                                    Convert.ToInt32((DateTime.Now - postedDate)
                                        .TotalDays); //datetime.now should be max posted date

                                OpenItem openItem = new OpenItem
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
                                Logger.LogError(ex, ex.Message);
                            }
                    }
                }
            }

            for (int i = 0; i < report.DaysRange.Length; i++)
            {
                List<OpenItem> items;

                if (i + 1 < report.DaysRange.Length)
                    items = openItems.Where(it => it.DaysOverdue >= report.DaysRange[i] && it.DaysOverdue < report.DaysRange[i + 1])
                        .ToList();
                else
                    items = openItems.Where(it => it.DaysOverdue >= report.DaysRange[i]).ToList();

                daysRecordsPairs.Add(report.DaysRange[i], items);
            }

            string agingExcel = await CreateModifiedAgingExcel(report.TempReportPath, report.DaysRange);

            if (daysRecordsPairs.Any() && report.TempReportPath.ToLower().Contains("proofing"))
            {
                report.TempReportPath = agingExcel;
                return (report, await CreateCsvFile(daysRecordsPairs, ServiceScopeFactory));
            }

            return (report, new Dictionary<int, string>());
        }

        private async Task<string> CreateModifiedAgingExcel(string inputFile, int[] daysRange)
        {
            //ignore balance proofing reports
            if (inputFile.ToLower().Contains("proofing"))
                return inputFile;

            string inputFileName = Path.GetFileName(inputFile);

            string outputFilePath =
                Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory), "Aged_" + inputFileName);

            using (ExcelPackage package = new ExcelPackage(new FileInfo(inputFile)))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.First();

                int maxDateInt = 0;

                DateTime maxDate = DateTime.Now;

                try
                {
                    maxDateInt = sheet.Cells["D:D"].Max(c =>
                    {
                        if (int.TryParse(c.Value?.ToString(), out int result)) return result;

                        return 0;
                    });

                    maxDate = FromExcelSerialDate(maxDateInt);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error obtaining excel date");
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
                ExcelCellAddress start = sheet.Dimension.Start;
                ExcelCellAddress end = sheet.Dimension.End;

                for (int i = start.Row + 7; i <= end.Row; i++)
                {
                    string dateFromExcel = sheet.Cells[$"D{i}"].Value?.ToString();

                    if (dateFromExcel != null && int.TryParse(dateFromExcel, out int dateInt))
                    {
                        DateTime outputDate = FromExcelSerialDate(dateInt);

                        int diff = (maxDate - outputDate).Days;

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

        private async Task<Dictionary<int, string>> CreateCsvFile(Dictionary<int, List<OpenItem>> items,
            IServiceScopeFactory serviceScopeFactory)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            foreach (KeyValuePair<int, List<OpenItem>> group in items)
            {
                string tempFilePath = Path.Combine(await FileHelpers.GetTempPath(serviceScopeFactory),
                    DateTime.Now.ToString("yyyy_MM_dd_") + group.Key + "_Days_Overdue_.csv");

                using (StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        await csv.WriteRecordsAsync(group.Value);
                    }
                }

                dict.Add(group.Key, tempFilePath);
            }

            return dict;
        }

        private DateTime FromExcelSerialDate(int SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug
            return new DateTime(1899, 12, 31).AddDays(SerialDate);
        }
    }
}
