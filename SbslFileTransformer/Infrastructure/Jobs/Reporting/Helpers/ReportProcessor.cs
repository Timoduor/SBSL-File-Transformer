using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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

                List<Task> tasks = new List<Task>();

                foreach (var reportUser in unprocessedReports)
                {
                    Logger.LogInformation($"Processing reports for user {reportUser.Key}");

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
                }

                await Task.WhenAll(tasks);
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

                        if (report.Name.ToLower().Contains("proofing"))
                        {
                            Logger.LogInformation($"Skipping report {report.Name.ToUpper()} with ID {report.ReportId} as it is a balance proofing report");
                            return;
                        }

                        if (await ReportsDownloader.DownloadReportAndUpdateLocalPath(report))
                        {
                            var matchedEscalations = GetMatchedEscalations(report, escalations);

                            report.ModifiedReportPath = await GenerateModifiedExcelReport(report, matchedEscalations.Select(e => e.Value.DaysOverdue).ToArray());

                            var processed = await GenerateEscalationReports(report, matchedEscalations);

                            processedReports.AddRange(processed);

                            Logger.LogInformation($"Finished processing report {report.Name.ToUpper()} of size {report.Size} with ID {report.ReportId}");
                        }

                        count++;

                        processReportProgress.Report(count * 100 / reports.Count());
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error processing report {report.Name.ToUpper()} of size {report.Size} with ID {report.ReportId}");
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
                Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory), "AGED_" + inputFileName);

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
                    sheet.Cells["E6"].Style.Font.Bold = true;
                    sheet.Cells["E6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells["E6"].AutoFitColumns();

                    //set formula for cells
                    var start = sheet.Dimension.Start;
                    var end = sheet.Dimension.End;

                    for (var i = start.Row + 7; i <= end.Row; i++)
                    {
                        var dateFromExcel = sheet.Cells[$"D{i}"].Value?.ToString();

                        if (!string.IsNullOrEmpty(dateFromExcel))
                        {
                            if (!DateTime.TryParse(dateFromExcel, out var outputDate) &&
                                !DateTime.TryParseExact(dateFromExcel, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate) &&
                                !DateTime.TryParseExact(dateFromExcel, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out outputDate))
                            {
                                if (double.TryParse(dateFromExcel, out var doubleFromExcel))
                                {
                                    try
                                    {
                                        outputDate = DateTime.FromOADate(doubleFromExcel);
                                    }
                                    catch (Exception)
                                    {
                                        Logger.LogError($"Error parsing date {dateFromExcel} for report {report.Name} with ID {report.ReportId}");
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            var diff = (maxDate.Date - outputDate.Date).Days;

                            sheet.Cells[$"E{i}"].Value = diff;

                            sheet.Cells[$"E{i}"].Style.Numberformat.Format = "0";
                        }
                        sheet.Cells[$"E{i}"].Style.Font.Bold = true;

                        sheet.Cells[$"E{i}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
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
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var matchedConfiguration = new List<KeyValuePair<ReportModel, ReportConfiguration>>();

            var reportContent = GetReportContent(report.TempReportPath);

            report.ReportContent = reportContent;

            var reportColumnTokens = reportContent.Where(x => x != null).SelectMany(x => new string[]
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

            Logger.LogInformation($"It took {sw.ElapsedMilliseconds} to extract {reportTextTokens.Count} tokens from report {report.Name.ToUpper()}");
            Logger.LogInformation($"Found {matchedConfiguration.Count} escalations for report {report.Name.ToUpper()}");

            return matchedConfiguration;
        }

        private List<OpenItem> GetReportContent(string path)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var openItems = new ConcurrentBag<OpenItem>();

            var maxDate = DateTime.Now;

            maxDate = DateTime.Now.DayOfWeek == DayOfWeek.Monday ? maxDate.AddDays(-2) : maxDate.AddDays(-1);

            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                        var tables = dataSet.Tables.Cast<DataTable>();

                        Parallel.ForEach(tables, table =>
                        {
                            var rows = table.Rows.Cast<DataRow>();

                            Parallel.ForEach(rows, row =>
                            {
                                try
                                {
                                    var openItem = new OpenItem();

                                    var colCount = row.Table.Columns.Count;

                                    if (colCount > 0)
                                        openItem.Account = row.ItemArray[0]?.ToString().Trim();

                                    if (colCount > 1)
                                        openItem.Entity = row.ItemArray[1]?.ToString().Trim();

                                    if (colCount > 2)
                                        openItem.AccName = row.ItemArray[2]?.ToString().Trim();

                                    if (colCount > 3)
                                        openItem.AccName = row.ItemArray[3]?.ToString().Trim();

                                    if (colCount > 4)
                                        openItem.Amount = row.ItemArray[4]?.ToString().Trim();

                                    if (colCount > 5)
                                        openItem.ItemSubType = row.ItemArray[5]?.ToString().Trim();

                                    if (colCount > 6)
                                        openItem.WeBalance = row.ItemArray[6]?.ToString().Trim();

                                    if (colCount > 7)
                                        openItem.TheyBalance = row.ItemArray[7]?.ToString().Trim();

                                    if (colCount > 8)
                                        openItem.ItemSide = row.ItemArray[8]?.ToString().Trim();

                                    if (colCount > 9)
                                        openItem.TransNarrative = row.ItemArray[9]?.ToString().Trim();

                                    if (colCount > 10)
                                        openItem.Reference1 = row.ItemArray[10]?.ToString().Trim();

                                    if (colCount > 11)
                                        openItem.Reference2 = row.ItemArray[11]?.ToString().Trim();

                                    if (colCount > 12)
                                        openItem.Reference3 = row.ItemArray[12]?.ToString().Trim();

                                    if (colCount > 13)
                                        openItem.ActiveCertStatus = row.ItemArray[13]?.ToString().Trim();

                                    if (colCount > 14)
                                        openItem.FunctionalArea = row.ItemArray[14]?.ToString().Trim();

                                    if (colCount > 15)
                                        openItem.ItemId = row.ItemArray[15]?.ToString().Trim();

                                    if (colCount > 16)
                                        openItem.Column16 = row.ItemArray[16]?.ToString().Trim();

                                    if (colCount > 17)
                                        openItem.Column17 = row.ItemArray[17]?.ToString().Trim();

                                    if (colCount > 18)
                                        openItem.Column18 = row.ItemArray[18]?.ToString().Trim();

                                    if (colCount > 19)
                                        openItem.Column19 = row.ItemArray[19]?.ToString().Trim();

                                    if (colCount > 20)
                                        openItem.Column20 = row.ItemArray[20]?.ToString().Trim();

                                    openItems.Add(openItem);
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError(ex, "Error fetching columns out of report");
                                }
                            });
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error fetching report content for report Path: {path}");
            }

            return openItems.ToList();
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

                        escalationReport.OverdueReportPath = await CreateEscalationReportFile(daysOverdue, report);

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

        private async Task<string> CreateEscalationReportFile(int daysOverdue, ReportModel report)
        {
            var tempFilePath = Path.Combine(await FileHelpers.GetTempPath(ServiceScopeFactory),
                $"Escalation_{DateTime.Now.ToString("yyyy_MM_dd_")}_{RandomNumberGen2.Next()}_{report.Name}_{daysOverdue}_Days_Overdue_.xlsx").ToUpper();

            //DELETE ALL OTHER ENTRIES THAT ARE NOT IN THE OVERDUE DAYS
            using (var package = new ExcelPackage(new FileInfo(report.ModifiedReportPath)))
            {
                var sheet = package.Workbook.Worksheets.First();

                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                for (var i = start.Row + 7; i <= end.Row; i++)
                {
                    if (int.TryParse(sheet.Cells[$"E{i}"].Value?.ToString(), out var calculatedDaysOverdue))
                    {
                        if (calculatedDaysOverdue < daysOverdue)
                        {
                            sheet.DeleteRow(i);
                            i--;
                        }
                    }
                }

                await package.SaveAsAsync(new FileInfo(tempFilePath));
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

                    string[] attachments = report.Escalation.IsManagerReport ? new[] { report.OverdueReportPath } :
                        new[] { report.OverdueReportPath, report.OriginalReport.ModifiedReportPath, report.OriginalReport.TempReportPath };

                    await emailSender.SendMessage(recipients, ReportConfigModel.EmailHeader + $" Report ID: {report.OriginalReport.ReportId}",
                                                        ReportConfigModel.EmailBody + Environment.NewLine + $"{report.Escalation.DaysOverdue} Days overdue" +
                                                        Environment.NewLine +
                                                        $"Report Name {report.OriginalReport.Name}" + Environment.NewLine +
                                                        $"Report generated by: {report.OriginalReport.Creator}" + Environment.NewLine +
                                                        $"COMMENTS:- {report.OriginalReport.Notes}", false, attachments);

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
