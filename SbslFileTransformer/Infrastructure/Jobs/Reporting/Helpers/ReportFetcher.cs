using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SbslFileTransformer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers
{
    public class ReportFetcher
    {
        readonly ILogger<ReportEngineJob> Logger;
        readonly ReportConfigModel reportConfiguration;
        readonly IHttpClientFactory HttpClientFactory;

        public ReportFetcher(ILogger<ReportEngineJob> logger, IHttpClientFactory httpClientFactory, ReportConfigModel reportConfig)
        {
            this.Logger = logger;
            this.HttpClientFactory = httpClientFactory;
            this.reportConfiguration = reportConfig;
        }

        public async Task<Dictionary<string, IEnumerable<ReportModel>>> GetAllUnprocessedRecentReportsAsync(List<long> processedReportsIds, IProgress<int> progressReporter)
        {
            this.Logger.LogInformation("Fetching all recent reports...");

            Dictionary<string, string> userTokens = await this.GetUserLoginTokens();

            Dictionary<string, IEnumerable<ReportModel>> allUsersUnprocessedReports = new Dictionary<string, IEnumerable<ReportModel>>();

            int count = 0;

            foreach (KeyValuePair<string, string> userToken in userTokens)
            {
                KeyValuePair<string, IEnumerable<ReportModel>> userReports = await this.GetUserRecentReports(userToken);

                IEnumerable<ReportModel> unprocessedReports = userReports.Value.Where(r => !processedReportsIds.Contains(r.ReportId));

                allUsersUnprocessedReports.Add(userToken.Key, unprocessedReports);

                count++;

                progressReporter.Report(count * 100 / userTokens.Count);
            }

            return allUsersUnprocessedReports;
        }



        private async Task<Dictionary<string, string>> GetUserLoginTokens()
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

        private async Task<KeyValuePair<string, IEnumerable<ReportModel>>> GetUserRecentReports(KeyValuePair<string, string> userToken)
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
