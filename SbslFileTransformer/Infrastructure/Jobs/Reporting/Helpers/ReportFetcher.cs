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
            Logger = logger;
            HttpClientFactory = httpClientFactory;
            reportConfiguration = reportConfig;
        }

        public async Task<Dictionary<string, IEnumerable<ReportModel>>> GetAllUnprocessedRecentReportsAsync(List<ProcessedReport> processedReports, IProgress<int> progressReporter)
        {
            Logger.LogInformation("Fetching all recent reports...");

            Dictionary<string, string> userTokens = await GetUserLoginTokens();

            Dictionary<string, IEnumerable<ReportModel>> allUsersUnprocessedReports = new Dictionary<string, IEnumerable<ReportModel>>();

            int count = 0;

            foreach (KeyValuePair<string, string> userToken in userTokens)
            {
                KeyValuePair<string, IEnumerable<ReportModel>> userReports = await GetUserRecentReports(userToken);

                IEnumerable<ReportModel> unprocessedReports = userReports.Value.Where(r => !processedReports.Select(p => p.ReportId).Contains(r.ReportId));

                allUsersUnprocessedReports.Add(userToken.Key, unprocessedReports);

                count++;

                progressReporter.Report(count * 100 / userTokens.Count);
            }

            return allUsersUnprocessedReports;
        }



        private async Task<Dictionary<string, string>> GetUserLoginTokens()
        {
            Logger.LogInformation("Fetching User tokens");

            Dictionary<string, string> userTokens = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> user in reportConfiguration.UserNamesAndPasswords)
            {
                try
                {
                    HttpClient client = HttpClientFactory.CreateClient("BlackLine");

                    client.Timeout = TimeSpan.FromMinutes(10);

                    List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("scope", reportConfiguration.Scope),
                            new KeyValuePair<string, string>("username", user.Key),
                            new KeyValuePair<string, string>("password", user.Value),
                            new KeyValuePair<string, string>("client_id", reportConfiguration.ClientId),
                            new KeyValuePair<string, string>("client_secret", reportConfiguration.ClientSecret)
                        };

                    FormUrlEncodedContent formdata = new FormUrlEncodedContent(content);

                    HttpResponseMessage response = await client.PostAsync(reportConfiguration.TokenUrl, formdata);

                    if (response.IsSuccessStatusCode)
                    {
                        string respContent = await response.Content.ReadAsStringAsync();

                        JObject data = JObject.Parse(respContent);

                        userTokens.Add(user.Key, (string)data.SelectToken("access_token"));
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            if (userTokens.Count() <= 0)
            {
                string users = string.Join($"{Environment.NewLine}", reportConfiguration.UserNamesAndPasswords.Select(u => u.Key));

                throw new ReportTokenFetchException($"No tokens were found for the specified users {Environment.NewLine} " +
                    $"{users}." +
                    $"{Environment.NewLine} Verify that their passwords are updated and report settings are correct");
            }

            return userTokens;
        }

        private async Task<KeyValuePair<string, IEnumerable<ReportModel>>> GetUserRecentReports(KeyValuePair<string, string> userToken)
        {
            Logger.LogInformation($"Fetching reports for {userToken.Key}");

            string reportsUrl = @$"https://{reportConfiguration.EnvironmentUrl}.{reportConfiguration.BaseUrl}/queryruns";

            List<ReportModel> reports = new List<ReportModel>();

            try
            {
                HttpClient client = HttpClientFactory.CreateClient("BlackLine");

                client.Timeout = TimeSpan.FromMinutes(10);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken.Value);

                HttpResponseMessage response = await client.GetAsync(reportsUrl);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    dynamic data = JArray.Parse(result);

                    foreach (dynamic item in data)
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
                            UserToken = userToken.Value
                        });
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }

            return new KeyValuePair<string, IEnumerable<ReportModel>>(userToken.Key, reports);
        }
    }
}
