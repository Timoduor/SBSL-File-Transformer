using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Encryption;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// Get emails that should get the report based on country/sprint/category
        /// </summary>
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
                    emails.AddRange(@group.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

                return emails;
            }
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
                        sheet.Cells[$"E{i}"].Value = (1 * result).ToString("N2");//change the (1 * result) to (-1 * result) if need be

                await package.SaveAsAsync(new FileInfo(outputFilePath));
            }

            return outputFilePath;
        }
    }
}