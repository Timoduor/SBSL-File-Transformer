using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Models.ViewModels;
using SbslFileTransformer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using SbslFileTransformer.Infrastructure.Helpers;
using System.Linq;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.HelpersV2.Interfaces;
using SbslFileTransformer.Infrastructure.Messaging;
using ExcelDataReader;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using OfficeOpenXml;
using System.Drawing;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.HelpersV2
{
    public class ReportProcessor : IReportProcessor
    {
        readonly ILogger<ReportEngineJobV2> Logger;
        readonly IServiceScopeFactory ServiceScopeFactory;
        readonly ReportConfigModel ReportConfigModel;
        readonly IReportsDownloader ReportsDownloader;

        public ReportProcessor(ILogger<ReportEngineJobV2> logger, IServiceScopeFactory serviceScopeFactory, IReportsDownloader reportsDownloader)
        {
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.ReportsDownloader = reportsDownloader;
            this.ReportConfigModel = reportsDownloader.LoadReportConnectionConfig();
        }

        public async Task ProcessFetchedReportsAsync(Dictionary<string, IEnumerable<ReportModel>> unprocessedReports, IProgress<int> processReportProgress)
        {
            using (IServiceScope scope = this.ServiceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                EmailSender emailSender = scope.ServiceProvider.GetService<EmailSender>();

                List<ReportConfiguration> escalations = await dbContext.ReportConfigurations.Where(e => e.IsEnabled).ToListAsync();

                foreach (KeyValuePair<string, IEnumerable<ReportModel>> reportUser in unprocessedReports)
                {
                    this.Logger.LogInformation($"Processing reports for user {reportUser.Key}");

                    foreach (IEnumerable<ReportModel> reportBatch in reportUser.Value.Batch(5))
                    {
                        try
                        {
                            var processed = await this.ProcessReportBatchAsync(reportBatch, processReportProgress, escalations);

                            await SendGeneratedEscalations(processed, emailSender);

                            await SaveProcessedEscalations(processed, dbContext);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError(ex, $"Error processing report batch for user {reportUser.Key}");
                        }
                    }
                }
            }
        }


        private async Task<List<EscalationReport>> ProcessReportBatchAsync(IEnumerable<ReportModel> reports, IProgress<int> processReportProgress, List<ReportConfiguration> escalations)
        {
            List<EscalationReport> processedReports = new List<EscalationReport>();

            int count = 0;

            foreach (ReportModel report in reports)
            {
                try
                {
                    this.Logger.LogInformation($"Processing report {report.Name} with ID {report.ReportId}");

                    if (await this.ReportsDownloader.DownloadReportAndUpdateLocalPath(report))
                    {
                        List<KeyValuePair<ReportModel, ReportConfiguration>> matchedEscalations = GetMatchedEscalations(report, escalations);

                        string modifiedExcel = await GenerateModifiedExcelReport(report, matchedEscalations.Select(e => e.Value.DaysOverdue).ToArray());

                        report.ModifiedReportPath = modifiedExcel;

                        List<EscalationReport> processed = await GenerateEscalationReports(report, matchedEscalations);

                        processedReports.AddRange(processed);
                    }

                    count++;

                    processReportProgress.Report(count * 100 / reports.Count());
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error processing report {report.Name} with ID {report.ReportId}");
                }
            }

            return processedReports;
        }

        private async Task<string> GenerateModifiedExcelReport(ReportModel report, int[] daysOverdue)
        {

            string inputFile = report.TempReportPath;

            //ignore balance proofing reports
            if (inputFile.ToLower().Contains("proofing"))
                return inputFile;

            string inputFileName = Path.GetFileName(inputFile);

            string outputFilePath =
                Path.Combine(await FileHelpers.GetTempPath(this.ServiceScopeFactory), "Aged_" + inputFileName);

            try
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(inputFile)))
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets.First();

                    DateTime maxDate = DateTime.Now;

                    maxDate = DateTime.Now.DayOfWeek == DayOfWeek.Monday ? maxDate.AddDays(-2) : maxDate.AddDays(-1);

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

                        if (!string.IsNullOrEmpty(dateFromExcel))
                        {
                            if (!DateTime.TryParse(dateFromExcel, out DateTime outputDate))
                            {
                                if (double.TryParse(dateFromExcel, out double doubleFromExcel))
                                {
                                    outputDate = DateTime.FromOADate(doubleFromExcel);
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            int diff = (maxDate.Date - outputDate.Date).Days;

                            sheet.Cells[$"E{i}"].Value = diff;

                            sheet.Cells[$"E{i}"].Style.Numberformat.Format = "0";


                            if (daysOverdue.Length >= 4)
                            {
                                if (diff >= daysOverdue[0] && diff <= daysOverdue[1])
                                    sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.GreenYellow);

                                if (diff > daysOverdue[1] && diff <= daysOverdue[2])
                                    sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.RosyBrown);

                                if (diff > daysOverdue[2] && diff <= daysOverdue[3])
                                    sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.Yellow);

                                if (diff > daysOverdue[3])
                                    sheet.Cells[$"E{i}"].Style.Fill.SetBackground(Color.Red);
                            }
                        }
                    }

                    //save new excel
                    await package.SaveAsAsync(new FileInfo(outputFilePath));
                }
            }
            catch (Exception ex)
            {
                File.Create(outputFilePath).Close();
                this.Logger.LogError(ex, $"Error Creating modified Aging Excel file {outputFilePath} for input file: {inputFile} with ID: {report.ReportId}");
            }

            return outputFilePath;
        }

        /// <summary>
        /// get the escalations that match the keywords for this report
        /// </summary>
        /// <param name="report"></param>
        /// <param name="escalations"></param>
        /// <returns></returns>
        private List<KeyValuePair<ReportModel, ReportConfiguration>> GetMatchedEscalations(ReportModel report, List<ReportConfiguration> escalations)
        {

            List<KeyValuePair<ReportModel, ReportConfiguration>> matchedConfiguration = new List<KeyValuePair<ReportModel, ReportConfiguration>>();

            List<OpenItem> reportContent = GetReportContent(report.TempReportPath);

            report.ReportContent = reportContent;

            List<string> reportColumnTokens = reportContent.SelectMany(x => new string[]
                        {
                            x.AccName,
                            x.ActiveCertStatus,
                            x.Amount,
                            x.Column16,
                            x.Column17,
                            x.Column18,
                            x.Column19,
                            x.Column20,
                            x.Entity,
                            x.FunctionalArea,
                            x.ItemId,
                            x.ItemSide,
                            x.Reference3,
                            x.Reference1,
                            x.Reference2,
                            x.WeBalance,
                            x.TheyBalance,
                            x.TransNarrative,
                            x.ItemSubType
                        }).Select(t => t?.ToLower()).ToList();

            List<string> reportTextTokens = new List<string>();

            foreach (var columnToken in reportColumnTokens)
            {
                reportTextTokens.AddRange(columnToken.Split(new char[] { ' ', '\t', '\n', '\r' }));
            }

            foreach (ReportConfiguration escalation in escalations)
            {
                try
                {
                    List<string> reportNameTokens = report.Name.Split(' ').Select(n => n.ToLower()).ToList();
                    List<string> escalationNameTokens = escalation.NameKeywords.Split(',').Select(n => n.ToLower()).ToList();
                    List<string> escalationColumnTokens = escalation.ColumnKeywords.Split(',').Select(c => c.ToLower()).ToList();

                    if (reportNameTokens.ContainsAllItems(escalationNameTokens)
                        && reportTextTokens.ContainsAllItems(escalationColumnTokens))
                    {
                        matchedConfiguration.Add(new KeyValuePair<ReportModel, ReportConfiguration>(report, escalation));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error fetching matched esclations for report {report.Name}");
                }
            }

            return matchedConfiguration;
        }

        private List<OpenItem> GetReportContent(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            List<OpenItem> openItems = new List<OpenItem>();

            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DateTime maxDate = DateTime.Now;

                        maxDate = DateTime.Now.DayOfWeek == DayOfWeek.Monday ? maxDate.AddDays(-2) : maxDate.AddDays(-1);

                        while (reader.Read())
                        {
                            string col3 = string.Empty;

                            if (reader.TryGetValue(3, out object postedDateString))
                            {
                                if (!string.IsNullOrEmpty(postedDateString?.ToString()))
                                    col3 = postedDateString?.ToString();
                            }

                            if (string.IsNullOrEmpty(col3))
                                continue;

                            if (DateTime.TryParseExact(col3, new string[2] { "M/d/yyyy", "MM/dd/yyyy" }, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime postedDate))
                            {
                                try
                                {
                                    int daysOverdue = Convert.ToInt32((maxDate.Date - postedDate.Date).Days);

                                    OpenItem openItem = new OpenItem
                                    {
                                        DaysOverdue = daysOverdue,
                                        PostedDate = postedDate
                                    };

                                    if (reader.TryGetValue(1, out object entity))
                                        openItem.Entity = entity?.ToString();

                                    if (reader.TryGetValue(2, out object accName))
                                        openItem.AccName = accName?.ToString();

                                    if (reader.TryGetValue(4, out object amount))
                                        openItem.Amount = amount?.ToString();

                                    if (reader.TryGetValue(5, out object itemSubType))
                                        openItem.ItemSubType = itemSubType?.ToString();

                                    if (reader.TryGetValue(6, out object weBalance))
                                        openItem.WeBalance = weBalance?.ToString();

                                    if (reader.TryGetValue(7, out object theyBalance))
                                        openItem.TheyBalance = theyBalance?.ToString();

                                    if (reader.TryGetValue(8, out object itemSide))
                                        openItem.ItemSide = itemSide?.ToString();

                                    if (reader.TryGetValue(9, out object transNarrative))
                                        openItem.TransNarrative = transNarrative?.ToString();

                                    if (reader.TryGetValue(10, out object ref1))
                                        openItem.Reference1 = ref1?.ToString();

                                    if (reader.TryGetValue(11, out object ref2))
                                        openItem.Reference2 = ref2?.ToString();

                                    if (reader.TryGetValue(12, out object ref3))
                                        openItem.Reference3 = ref3?.ToString();

                                    if (reader.TryGetValue(14, out object activeStatus))
                                        openItem.ActiveCertStatus = activeStatus?.ToString();

                                    if (reader.TryGetValue(13, out object funcArea))
                                        openItem.FunctionalArea = funcArea?.ToString();

                                    if (reader.TryGetValue(15, out object itemId))
                                        openItem.ItemId = itemId?.ToString();

                                    if (reader.TryGetValue(16, out object itemId16))
                                        openItem.Column16 = itemId16?.ToString();

                                    if (reader.TryGetValue(17, out object itemId17))
                                        openItem.Column17 = itemId17?.ToString();

                                    if (reader.TryGetValue(18, out object itemId18))
                                        openItem.Column18 = itemId18?.ToString();

                                    if (reader.TryGetValue(19, out object itemId19))
                                        openItem.Column19 = itemId19?.ToString();

                                    if (reader.TryGetValue(20, out object itemId20))
                                        openItem.Column20 = itemId20?.ToString();

                                    openItems.Add(openItem);
                                }
                                catch (Exception ex)
                                {
                                    this.Logger.LogError(ex, "Error fetching columns out of report");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching report content for report Path: {path}");
            }

            return openItems;
        }

        /// <summary>
        /// This reads through the report calculates the date for a transaction and adds the overdue days
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private async Task<List<EscalationReport>> GenerateEscalationReports(ReportModel report, List<KeyValuePair<ReportModel, ReportConfiguration>> matchedEscalations)
        {
            List<EscalationReport> escalationReports = new List<EscalationReport>();

            foreach (var escalation in matchedEscalations)
            {
                try
                {
                    EscalationReport escalationReport = new EscalationReport();

                    escalationReport.OriginalReport = report;
                    escalationReport.Escalation = escalation.Value;

                    int daysOverdue = escalation.Value.DaysOverdue;

                    List<OpenItem> overdueItems = escalation.Key.ReportContent.Where(i => i.DaysOverdue >= daysOverdue).OrderBy(i => i.DaysOverdue).ToList();

                    escalationReport.OverdueReportPath = await CreateCsvFiles(overdueItems, daysOverdue, report.Name);

                    escalationReports.Add(escalationReport);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error generating escalation reports");
                }
            }

            return escalationReports;
        }

        private async Task<string> CreateCsvFiles(List<OpenItem> items, int daysOverdue, string reportName)
        {
            string tempFilePath = Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory),
                $"{DateTime.Now.ToString("yyyy_MM_dd_")}_{reportName}_{daysOverdue}_Days_Overdue_.csv");

            using (StreamWriter writer = new StreamWriter(tempFilePath))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(items);
                }
            }

            return tempFilePath;
        }

        private async Task SaveProcessedEscalations(List<EscalationReport> processed, ApplicationDbContext dbContext)
        {
            try
            {
                IEnumerable<long> reportIds = processed.Select(p => p.OriginalReport.ReportId).Distinct();

                var newProcessed = new List<EscalationReport>();

                foreach (long reportId in reportIds)
                {
                    var first = processed.FirstOrDefault(p => p.OriginalReport.ReportId == reportId);

                    newProcessed.Add(first);
                }

                foreach (var report in newProcessed)
                {
                    if (!dbContext.ProcessedReports.Any(p => p.Id == report.OriginalReport.ReportId))
                    {
                        await dbContext.ProcessedReports.AddAsync(new ProcessedReport()
                        {
                            ReportId = report.OriginalReport.ReportId,
                            Creator = report.OriginalReport.Creator,
                            EndTime = report.OriginalReport.EndTime,
                            Message = report.OriginalReport.Message,
                            Name = report.OriginalReport.Name,
                            ProcessedDate = DateTime.Now,
                            Notes = report.OriginalReport.Notes,
                            StartTime = report.OriginalReport.StartTime,
                            Status = report.OriginalReport.Status
                        });
                    }
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error saving processed report batch with report IDs: {string.Join(",", processed.Select(p => p.OriginalReport.ReportId).ToList())}");
            }
        }

        private async Task SendGeneratedEscalations(List<EscalationReport> processed, EmailSender emailSender)
        {
            foreach (var report in processed)
            {
                try
                {
                    List<string> recipients = report.Escalation.RecipientEmails.Split(',').ToList();

                    await emailSender.SendMessage(recipients, ReportConfigModel.EmailHeader + $" Report ID: {report.OriginalReport.ReportId}",
                                                        this.ReportConfigModel.EmailBody + Environment.NewLine + $"{report.Escalation.DaysOverdue} Days overdue" +
                                                        Environment.NewLine +
                                                        $"Report Name {report.OriginalReport.Name}" + Environment.NewLine +
                                                        $"Report generated by: {report.OriginalReport.Creator}" + Environment.NewLine +
                                                        $"COMMENTS:- {report.OriginalReport.Notes}", false,
                                    new[] { report.OriginalReport.TempReportPath, report.OriginalReport.ModifiedReportPath, report.OverdueReportPath });
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, $"Error sending report with ID: {report.OriginalReport.ReportId}");
                }
            }
        }
    }
}
