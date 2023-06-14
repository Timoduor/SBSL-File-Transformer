using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers.Interfaces;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers
{
    public class ReportListDownloader : IReportsDownloader
    {
        readonly ILogger<ReportListDownloader> Logger;
        readonly ReportConfigModel reportConfiguration;
        readonly IHttpClientFactory HttpClientFactory;
        readonly IServiceScopeFactory ServiceScopeFactory;

        public ReportListDownloader(ILogger<ReportListDownloader> logger, IHttpClientFactory httpClientFactory, IServiceScopeFactory serviceScopeFactory)
        {
            this.Logger = logger;
            this.HttpClientFactory = httpClientFactory;
            this.ServiceScopeFactory = serviceScopeFactory;
            this.reportConfiguration = LoadReportConnectionConfig();
        }

        /// <summary>
        /// Gets a list of all unprocessed reports in the last month
        /// </summary>
        /// <param name="processedReportsIds"></param>
        /// <param name="progressReporter"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, IEnumerable<ReportModel>>> GetUnprocessedReportListAsync(List<long> processedReportsIds, IProgress<int> progressReporter)
        {
            this.Logger.LogInformation("Fetching all recent reports...");

            Dictionary<string, string> userTokens = await this.GetUserLoginTokensAsync();

            Dictionary<string, IEnumerable<ReportModel>> allUsersUnprocessedReports = new Dictionary<string, IEnumerable<ReportModel>>();

            int count = 0;

            foreach (KeyValuePair<string, string> userToken in userTokens)
            {
                KeyValuePair<string, IEnumerable<ReportModel>> userReports = await this.GetUserRecentReportListAsync(userToken);

                Logger.LogInformation($"User: {userToken.Key} has {userReports.Value.Count()} reports");

                IEnumerable<ReportModel> unprocessedReports = userReports.Value.Where(r => !processedReportsIds.Contains(r.ReportId));

                allUsersUnprocessedReports.Add(userToken.Key, unprocessedReports);

                count++;

                progressReporter.Report(count * 100 / userTokens.Count);
            }

            return allUsersUnprocessedReports;
        }

        /// <summary>
        /// Download the report specified in the reportModel and update its local path in the object
        /// </summary>
        /// <param name="report">Report to be downloaded and local path updated</param>
        /// <returns></returns>
        public async Task<bool> DownloadReportAndUpdateLocalPath(ReportModel report)
        {
            this.Logger.LogInformation($"Downloading report ID: {report.ReportId} Title: {report.Name}");

            string tempFolder = Path.Combine(await FileHelpers.GetTempPath(this.ServiceScopeFactory), "Reports");

            Directory.CreateDirectory(tempFolder);

            var reportConfigModel = LoadReportConnectionConfig();

            //set the local path that the report will be saved locally for processing
            report.TempReportPath = Path.Combine(tempFolder,
                                $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_{RandomNumberGen2.Next()}_{report.Name}." +
                                (reportConfigModel.ExportType == "Excel" ? "xlsx" : reportConfigModel.ExportType));

            string reportToDownload =
                @$"https://{reportConfigModel.EnvironmentUrl}.{reportConfigModel.BaseUrl}/completedqueryrun/{report.ReportId}/{reportConfigModel.ExportType}";
            try
            {
                HttpClient client = this.HttpClientFactory.CreateClient("BlackLine");

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

                    report.Size = new FileInfo(report.TempReportPath).Length;

                    return true;
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Gets report configuration used to download reports
        /// </summary>
        /// <returns></returns>
        public ReportConfigModel LoadReportConnectionConfig()
        {
            Logger.LogInformation("Fetching report connection configuration");

            using (IServiceScope scope = this.ServiceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                List<Configuration> reportConfigurations = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.Report)
                    .ToList();

                List<Configuration> userLogins = dbContext.Configurations.Where(c => c.ConfigType == ConfigurationType.ReportUser)
                    .ToList();

                ReportConfigModel config = new ReportConfigModel
                {
                    BaseUrl = reportConfigurations.FirstOrDefault(c => c.Key == "BaseUrl")?.Value,
                    EnvironmentUrl = reportConfigurations.FirstOrDefault(c => c.Key == "EnvironmentUrl")?.Value,
                    UserToken = reportConfigurations.FirstOrDefault(c => c.Key == "UserToken")?.Value,
                    EmailBody = reportConfigurations.FirstOrDefault(c => c.Key == "EmailBody")?.Value,
                    EmailHeader = reportConfigurations.FirstOrDefault(c => c.Key == "EmailHeader")?.Value,
                    ExportType = reportConfigurations.FirstOrDefault(c => c.Key == "ExportType")?.Value,
                    Scope = reportConfigurations.FirstOrDefault(c => c.Key == "Scope")?.Value,
                    TokenUrl = reportConfigurations.FirstOrDefault(c => c.Key == "TokenUrl")?.Value,
                    ClientId = reportConfigurations.FirstOrDefault(c => c.Key == "ClientId")?.Value,
                    ClientSecret = reportConfigurations.FirstOrDefault(c => c.Key == "ClientSecret")?.Value,
                    UserNamesAndPasswords = new Dictionary<string, string>()
                };

                foreach (Configuration login in userLogins)
                {
                    config.UserNamesAndPasswords.Add(login.Key, login.Value);
                }

                return config;
            }
        }

        /// <summary>
        /// Get User tokens from Configurations table of cofigtype ReportUser {5}
        /// </summary>
        /// <returns>Dictionary of userIds and tokenValues</returns>
        private async Task<Dictionary<string, string>> GetUserLoginTokensAsync()
        {
            this.Logger.LogInformation("Fetching User tokens");

            Dictionary<string, string> userTokens = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> user in this.reportConfiguration.UserNamesAndPasswords)
            {
                try
                {
                    HttpClient client = this.HttpClientFactory.CreateClient("BlackLine");

                    client.Timeout = TimeSpan.FromMinutes(10);

                    List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("scope", this.reportConfiguration.Scope?.Trim()),
                            new KeyValuePair<string, string>("username", user.Key?.Trim()),
                            new KeyValuePair<string, string>("password", user.Value?.Trim()),
                            new KeyValuePair<string, string>("client_id", this.reportConfiguration.ClientId?.Trim()),
                            new KeyValuePair<string, string>("client_secret", this.reportConfiguration.ClientSecret?.Trim())
                        };

                    FormUrlEncodedContent formdata = new FormUrlEncodedContent(content);

                    HttpResponseMessage response = await client.PostAsync(this.reportConfiguration.TokenUrl, formdata);

                    if (response.IsSuccessStatusCode)
                    {
                        string respContent = await response.Content.ReadAsStringAsync();

                        JObject data = JObject.Parse(respContent);

                        userTokens.Add(user.Key?.Trim(), (string)data.SelectToken("access_token"));
                    }

                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, ex.Message);
                }
            }

            if (!userTokens.Any())
            {
                string users = string.Join($"{Environment.NewLine}", this.reportConfiguration.UserNamesAndPasswords.Select(u => u.Key));

                throw new ReportTokenFetchException($"No tokens were found for the specified users {Environment.NewLine} " +
                    $"{users}." +
                    $"{Environment.NewLine} Verify that their passwords are updated and report settings are correct");
            }

            return userTokens;
        }

        /// <summary>
        /// Gets a list of recent reports for the specified user token
        /// </summary>
        /// <param name="userToken">KeyValuePair of userId and token value</param>
        /// <returns></returns>
        private async Task<KeyValuePair<string, IEnumerable<ReportModel>>> GetUserRecentReportListAsync(KeyValuePair<string, string> userToken)
        {
            this.Logger.LogInformation($"Fetching reports for {userToken.Key}");

            string reportsUrl = @$"https://{this.reportConfiguration.EnvironmentUrl}.{this.reportConfiguration.BaseUrl}/queryruns";

            List<ReportModel> reports = new List<ReportModel>();

            try
            {
                HttpClient client = this.HttpClientFactory.CreateClient("BlackLine");

                client.Timeout = TimeSpan.FromMinutes(10);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken.Value);

                HttpResponseMessage response = await client.GetAsync(reportsUrl);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    dynamic data = JArray.Parse(result);

                    foreach (dynamic item in data)
                    {
                        var reportItem = new ReportModel
                        {
                            Creator = item.creatorFirstAndLastName,
                            EndTime = item.endTime,
                            Message = item.message,
                            Name = item.name,
                            Notes = item.notes,
                            ReportId = item.id,
                            StartTime = item.startTime,
                            Status = item.status,
                            UserToken = userToken.Value,
                        };

                        if (DateTime.TryParse(item?.endTime?.ToString(), out DateTime reportDate))
                        {
                            reportItem.ReportDate = reportDate;
                        }
                        else
                        {
                            reportItem.ReportDate = DateTime.Now;
                        }

                        reports.Add(reportItem);
                    }
                }

            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, ex.Message);
            }

            return new KeyValuePair<string, IEnumerable<ReportModel>>(userToken.Key, reports.Where(r => r.ReportDate > DateTime.Now.AddDays(-31)));
        }
    }
}
