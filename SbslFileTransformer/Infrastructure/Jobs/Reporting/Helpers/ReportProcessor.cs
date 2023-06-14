using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers.Interfaces;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers
{
    public class ReportProcessor : IReportProcessor
    {
        readonly ILogger<ReportEngineJob> Logger;
        readonly IServiceScopeFactory ServiceScopeFactory;
        readonly ReportConfigModel ReportConfigModel;
        readonly IReportsDownloader ReportsDownloader;

        public ReportProcessor(ILogger<ReportEngineJob> logger, IServiceScopeFactory serviceScopeFactory, IReportsDownloader reportsDownloader)
        {
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
            ReportsDownloader = reportsDownloader;
            ReportConfigModel = reportsDownloader.LoadReportConnectionConfig();
        }

        public async Task ProcessFetchedReportsAsync(Dictionary<string, IEnumerable<ReportModel>> unprocessedReports, IProgress<int> processReportProgress)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var emailSender = scope.ServiceProvider.GetService<EmailSender>();

                var escalations = await dbContext.ReportConfigurations.Where(e => e.IsEnabled).ToListAsync();

                foreach (var reportUser in unprocessedReports)
                {
                    Logger.LogInformation($"Processing reports for user {reportUser.Key}");

                    List<Task> tasks = new List<Task>();

                    foreach (var reportBatch in reportUser.Value.Batch(5))
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await SaveProcessedEscalations(reportBatch, ServiceScopeFactory);

                                var processed = await ProcessReportBatchAsync(reportBatch, processReportProgress, escalations);

                                await SendGeneratedEscalations(processed, emailSender);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, $"Error processing report batch for user {reportUser.Key}");
                            }
                        }));
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }


        private async Task<List<EscalationReport>> ProcessReportBatchAsync(IEnumerable<ReportModel> reports, IProgress<int> processReportProgress, List<ReportConfiguration> escalations)
        {
            var processedReports = new List<EscalationReport>();

            var count = 0;

            List<Task> reportTasks = new List<Task>();

            foreach (var report in reports)
            {
                reportTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        Logger.LogInformation($"Processing report {report.Name.ToUpper()} with ID {report.ReportId}");

                        if (await ReportsDownloader.DownloadReportAndUpdateLocalPath(report))
                        {
                            var matchedEscalations = GetMatchedEscalations(report, escalations);

                            report.ModifiedReportPath = await GenerateModifiedExcelReport(report, matchedEscalations.Select(e => e.Value.DaysOverdue).ToArray());

                            var processed = await GenerateEscalationReports(report, matchedEscalations);

                            processedReports.AddRange(processed);

                            Logger.LogInformation($"Finished processing report {report.Name.ToUpper()} with ID {report.ReportId}");
                        }

                        count++;

                        processReportProgress.Report(count * 100 / reports.Count());
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error processing report {report.Name.ToUpper()} with ID {report.ReportId}");
                    }
                }));
            }

            await Task.WhenAll(reportTasks);

            return processedReports;
        }

        private async Task<string> GenerateModifiedExcelReport(ReportModel report, int[] daysOverdue)
        {

            var inputFile = report.TempReportPath;

            //ignore balance proofing reports
            if (inputFile.ToLower().Contains("proofing"))
                return inputFile;

            var inputFileName = Path.GetFileName(inputFile);

            var outputFilePath =
                Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory), "Aged_" + inputFileName);

            try
            {
                using (var package = new ExcelPackage(new FileInfo(inputFile)))
                {
                    var sheet = package.Workbook.Worksheets.First();

                    var maxDate = DateTime.Now;

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
                    var start = sheet.Dimension.Start;
                    var end = sheet.Dimension.End;

                    for (var i = start.Row + 7; i <= end.Row; i++)
                    {
                        var dateFromExcel = sheet.Cells[$"D{i}"].Value?.ToString();

                        if (!string.IsNullOrEmpty(dateFromExcel))
                        {
                            if (!DateTime.TryParse(dateFromExcel, out var outputDate))
                            {
                                if (double.TryParse(dateFromExcel, out var doubleFromExcel))
                                {
                                    outputDate = DateTime.FromOADate(doubleFromExcel);
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            var diff = (maxDate.Date - outputDate.Date).Days;

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
                Logger.LogError(ex, $"Error Creating modified Aging Excel file {outputFilePath} for input file: {inputFile} with Name: {report.Name.ToUpper()}");
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

            var matchedConfiguration = new List<KeyValuePair<ReportModel, ReportConfiguration>>();

            var reportContent = GetReportContent(report.TempReportPath);

            report.ReportContent = reportContent;

            var reportColumnTokens = reportContent.SelectMany(x => new string[]
                        {
                            x.Account,
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
                        }).Select(t => t?.ToLower().Trim()).ToList();

            var reportTextTokens = new List<string>();

            foreach (var columnToken in reportColumnTokens)
            {
                if (!string.IsNullOrEmpty(columnToken))
                    reportTextTokens.AddRange(columnToken.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
            }

            foreach (var escalation in escalations)
            {
                try
                {
                    var reportNameTokens = report.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim().ToLower()).ToList();
                    var escalationNameTokens = escalation.NameKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim().ToLower()).ToList();
                    var escalationColumnTokens = escalation.ColumnKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim().ToLower()).ToList();

                    if (reportNameTokens.ContainsAllItems(escalationNameTokens)
                        && reportTextTokens.ContainsAllItems(escalationColumnTokens))
                    {
                        matchedConfiguration.Add(new KeyValuePair<ReportModel, ReportConfiguration>(report, escalation));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error fetching matched esclations for report {report.Name.ToUpper()}");
                }
            }

            return matchedConfiguration;
        }

        private List<OpenItem> GetReportContent(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var openItems = new List<OpenItem>();

            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var maxDate = DateTime.Now;

                        maxDate = DateTime.Now.DayOfWeek == DayOfWeek.Monday ? maxDate.AddDays(-2) : maxDate.AddDays(-1);

                        while (reader.Read())
                        {
                            try
                            {
                                var openItem = new OpenItem();

                                if (reader.TryGetValue(0, out var account))
                                    openItem.Account = account?.ToString().Trim();

                                if (reader.TryGetValue(1, out var entity))
                                    openItem.Entity = entity?.ToString().Trim();

                                if (reader.TryGetValue(2, out var accName))
                                    openItem.AccName = accName?.ToString().Trim();

                                if (reader.TryGetValue(4, out var amount))
                                    openItem.Amount = amount?.ToString().Trim();

                                if (reader.TryGetValue(5, out var itemSubType))
                                    openItem.ItemSubType = itemSubType?.ToString().Trim();

                                if (reader.TryGetValue(6, out var weBalance))
                                    openItem.WeBalance = weBalance?.ToString().Trim();

                                if (reader.TryGetValue(7, out var theyBalance))
                                    openItem.TheyBalance = theyBalance?.ToString().Trim();

                                if (reader.TryGetValue(8, out var itemSide))
                                    openItem.ItemSide = itemSide?.ToString();

                                if (reader.TryGetValue(9, out var transNarrative))
                                    openItem.TransNarrative = transNarrative?.ToString().Trim();

                                if (reader.TryGetValue(10, out var ref1))
                                    openItem.Reference1 = ref1?.ToString().Trim();

                                if (reader.TryGetValue(11, out var ref2))
                                    openItem.Reference2 = ref2?.ToString().Trim();

                                if (reader.TryGetValue(12, out var ref3))
                                    openItem.Reference3 = ref3?.ToString().Trim();

                                if (reader.TryGetValue(14, out var activeStatus))
                                    openItem.ActiveCertStatus = activeStatus?.ToString().Trim();

                                if (reader.TryGetValue(13, out var funcArea))
                                    openItem.FunctionalArea = funcArea?.ToString().Trim();

                                if (reader.TryGetValue(15, out var itemId))
                                    openItem.ItemId = itemId?.ToString().Trim();

                                if (reader.TryGetValue(16, out var itemId16))
                                    openItem.Column16 = itemId16?.ToString().Trim();

                                if (reader.TryGetValue(17, out var itemId17))
                                    openItem.Column17 = itemId17?.ToString().Trim();

                                if (reader.TryGetValue(18, out var itemId18))
                                    openItem.Column18 = itemId18?.ToString().Trim();

                                if (reader.TryGetValue(19, out var itemId19))
                                    openItem.Column19 = itemId19?.ToString().Trim();

                                if (reader.TryGetValue(20, out var itemId20))
                                    openItem.Column20 = itemId20?.ToString().Trim();

                                openItems.Add(openItem);
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError(ex, "Error fetching columns out of report");
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
            var escalationReports = new List<EscalationReport>();

            List<Task> escalationTasks = new List<Task>();

            foreach (var escalation in matchedEscalations)
            {
                escalationTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var escalationReport = new EscalationReport();

                        escalationReport.OriginalReport = report;
                        escalationReport.Escalation = escalation.Value;

                        var daysOverdue = escalation.Value.DaysOverdue;

                        var overdueItems = escalation.Key.ReportContent.Where(i => i.DaysOverdue >= daysOverdue).OrderBy(i => i.DaysOverdue).ToList();

                        escalationReport.OverdueReportPath = await CreateCsvFiles(overdueItems, daysOverdue, report.Name);

                        escalationReports.Add(escalationReport);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error generating escalation reports");
                    }
                }));
            }

            await Task.WhenAll(escalationTasks);

            return escalationReports;
        }

        private async Task<string> CreateCsvFiles(List<OpenItem> items, int daysOverdue, string reportName)
        {
            var tempFilePath = Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory),
                $"{DateTime.Now.ToString("yyyy_MM_dd_")}_{RandomNumberGen2.Next()}_{reportName}_{daysOverdue}_Days_Overdue_.csv");

            using (var writer = new StreamWriter(tempFilePath))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(items);
                }
            }

            return tempFilePath;
        }

        private async Task SaveProcessedEscalations(IEnumerable<ReportModel> processed, IServiceScopeFactory serviceScopeFactory)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


                    var reportIds = processed.Select(p => p.ReportId).Distinct();

                    var newProcessed = new List<ReportModel>();

                    foreach (var reportId in reportIds)
                    {
                        var first = processed.FirstOrDefault(p => p.ReportId == reportId);

                        newProcessed.Add(first);
                    }

                    foreach (var report in newProcessed)
                    {
                        if (!dbContext.ProcessedReports.Any(p => p.Id == report.ReportId))
                        {
                            await dbContext.ProcessedReports.AddAsync(new ProcessedReport()
                            {
                                ReportId = report.ReportId,
                                Creator = report.Creator,
                                EndTime = report.EndTime,
                                Message = report.Message,
                                Name = report.Name,
                                ProcessedDate = DateTime.Now,
                                Notes = report.Notes,
                                StartTime = report.StartTime,
                                Status = report.Status
                            });
                        }
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error saving processed report batch with report IDs: {string.Join(",", processed.Select(p => p.ReportId).ToList())}");
            }
        }

        private async Task SendGeneratedEscalations(List<EscalationReport> processed, EmailSender emailSender)
        {
            foreach (var report in processed)
            {
                try
                {
                    var recipients = report.Escalation.RecipientEmails.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

                    await emailSender.SendMessage(recipients, ReportConfigModel.EmailHeader + $" Report ID: {report.OriginalReport.ReportId}",
                                                        ReportConfigModel.EmailBody + Environment.NewLine + $"{report.Escalation.DaysOverdue} Days overdue" +
                                                        Environment.NewLine +
                                                        $"Report Name {report.OriginalReport.Name}" + Environment.NewLine +
                                                        $"Report generated by: {report.OriginalReport.Creator}" + Environment.NewLine +
                                                        $"COMMENTS:- {report.OriginalReport.Notes}", false,
                                    new[] { report.OriginalReport.TempReportPath, report.OriginalReport.ModifiedReportPath, report.OverdueReportPath });

                    Logger.LogInformation($"Sent report with name: {report.OriginalReport.Name} to {report.Escalation.ReportDescription}");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error sending report with ID: {report.OriginalReport.ReportId} and Name: {report.OriginalReport.Name.ToUpper()}");
                }
            }
        }
    }
}
