using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting
{
    public partial class ScheduledReporterJob
    {
        public ReportConfigModel GetConfiguration(IServiceScopeFactory serviceScopeFactory)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var encryptionManager = scope.ServiceProvider.GetService<EncryptionManager>();

                var configurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report)
                    .ToList();

                var userLogins = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.ReportUser)
                    .ToList();

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
                    ClientSecret = configurations.FirstOrDefault(c => c.Key == "ClientSecret")?.Value
                };

                config.UserNamesAndPasswords = new Dictionary<string, string>();

                foreach (var login in userLogins) config.UserNamesAndPasswords.Add(login.Key, login.Value);

                return config;
            }
        }

        /// <summary>
        ///     Download individual report using the report's ID and save to the specified path
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="config"></param>
        /// <param name="savePath"></param>
        /// <param name="token"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task<bool> DownloadReport(long reportId, ReportConfigModel config, string savePath, string token,
            ILogger<ScheduledReporterJob> logger)
        {
            var reportToDownload =
                @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/completedqueryrun/{reportId}/{config.ExportType}";
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
                logger.LogError(ex, ex.Message);
            }

            return false;
        }

        /// <summary>
        ///     Get DateTime from Excel serial date value
        /// </summary>
        /// <param name="SerialDate"></param>
        /// <returns></returns>
        public DateTime FromExcelSerialDate(int SerialDate)
        {
            if (SerialDate > 59) SerialDate -= 1; //Excel/Lotus 2/29/1900 bug
            return new DateTime(1899, 12, 31).AddDays(SerialDate);
        }

        /// <summary>
        ///     Create a CSV file from the list of open items already grouped by days overdue
        /// </summary>
        /// <param name="items"></param>
        /// <param name="serviceScopeFactory"></param>
        /// <returns></returns>
        public async Task<Dictionary<int, string>> CreateCsvFile(Dictionary<int, List<OpenItem>> items,
            IServiceScopeFactory serviceScopeFactory)
        {
            var dict = new Dictionary<int, string>();

            foreach (var group in items)
            {
                var tempFilePath = Path.Combine(await FileHelpers.GetTempPath(serviceScopeFactory),
                    DateTime.Now.ToString("yyyy_MM_dd_") + group.Key + "_Days_Overdue_.csv");

                using (var writer = new StreamWriter(tempFilePath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        await csv.WriteRecordsAsync(group.Value);
                    }
                }

                dict.Add(group.Key, tempFilePath);
            }

            return dict;
        }

        /// <summary>
        ///     Get the various email groups based on the criteria returned in pair of days overdue
        /// </summary>
        /// <param name="emailGroups"></param>
        /// <param name="daysRange"></param>
        /// <param name="country"></param>
        /// <param name="sprint"></param>
        /// <param name="category"></param>
        public int[] GetEmailGroupDays(List<EmailGroup> emailGroups, Country country = Country.Kenya,
            Sprint sprint = Sprint.Nostro, ReportCategory category = ReportCategory.Default)
        {
            var groups = emailGroups.Where(g => g.Country == country && g.Sprint == sprint && g.Category == category);

            if (category == ReportCategory.Default)
                groups = emailGroups.Where(g => g.Country == country && g.Sprint == sprint);

            var daysRange = groups.OrderBy(g => g.AgeAlertDuration).Select(g => g.AgeAlertDuration).ToArray();

            return daysRange;
        }

        public static IEnumerable<string> GetEmails(int duration, Country country, Sprint sprint,
            ReportCategory category, IServiceScopeFactory serviceScopeFactory)
        {
            var emails = new List<string>();

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                var groups = dbContext.EmailGroups.Where(g =>
                    g.AgeAlertDuration == duration && g.Country == country && g.Sprint == sprint &&
                    g.Category == category && g.IsActive);

                if (category == ReportCategory.Default)
                    groups = dbContext.EmailGroups.Where(g =>
                        g.AgeAlertDuration == duration && g.Country == country && g.Sprint == sprint && g.IsActive);

                var groupEmails = groups.ToList().Select(g => g.Emails);

                foreach (var group in groupEmails)
                    emails.AddRange(@group.Split(new[] {',', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries));

                return emails;
            }
        }

        /// <summary>
        ///     Get login tokens for downloading reports from blackline
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetLoginTokens(ReportConfigModel config,
            ILogger<ScheduledReporterJob> logger)
        {
            var tokens = new List<string>();

            try
            {
                foreach (var user in config.UserNamesAndPasswords)
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
                            new KeyValuePair<string, string>("client_secret", config.ClientSecret)
                        };

                        var formdata = new FormUrlEncodedContent(content);

                        var response = await client.PostAsync(config.TokenUrl, formdata);

                        if (response.IsSuccessStatusCode)
                        {
                            var respContent = await response.Content.ReadAsStringAsync();

                            var data = JObject.Parse(respContent);

                            tokens.Add((string) data.SelectToken("access_token"));
                        }
                    }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            return tokens;
        }

        /// <summary>
        ///     Get latest reports from blackline website
        /// </summary>
        /// <param name="config"></param>
        /// <param name="token"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ReportModel>> GetRecentReports(ReportConfigModel config, string token,
            ILogger<ScheduledReporterJob> logger)
        {
            var reportsUrl = @$"https://{config.EnvironmentUrl}.{config.BaseUrl}/queryruns";

            var reports = new List<ReportModel>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.GetAsync(reportsUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        dynamic data = JArray.Parse(result);

                        foreach (var item in data)
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
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            return reports;
        }

        /// <summary>
        ///     changes the balance value in the Tanzania clearing suspense balance proofing report (* -1)
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns>Adjusted file path</returns>
        private async Task<string> AdjustBalanceValue(string inputFile)
        {
            var inputFileName = Path.GetFileName(inputFile);

            var outputFilePath =
                Path.Combine(await FileHelpers.GetTempPath(_serviceScopeFactory), "Adj_" + inputFileName);

            using (var package = new ExcelPackage(new FileInfo(inputFile)))
            {
                var sheet = package.Workbook.Worksheets.First();

                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                for (var i = start.Row + 5; i <= end.Row; i++)
                    if (double.TryParse(sheet.Cells[$"E{i}"].Value.ToString(), out var result))
                        sheet.Cells[$"E{i}"].Value = (-1 * result).ToString("N2");

                await package.SaveAsAsync(new FileInfo(outputFilePath));
            }

            return outputFilePath;
        }
    }
}