using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Models;
using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Converters
{
    public class BalanceFileConverter
    {
        private static readonly object _locker = new object();

        public BalanceFileConverter(ILogger logger, IServiceScopeFactory serviceScopeFactory, string entity)
        {
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
            Entity = entity;
        }

        public ILogger Logger { get; set; }

        public Guid Id => new Guid("701d74d6-bb48-4384-9d73-1466de46e61f");

        public string Name => "Finacle Balance File Converter";

        public string Description => "Converts finacle generated csv file to standard blackline tab separated file";

        public string OutputFolder { get; set; }
        public int StartDelay { get; set; }
        public bool IsManualRun { get; set; }
        public string Entity { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public async Task<bool> Execute(string filePath, string functionalArea = "Nostros")
        {
            try
            {
                if (string.IsNullOrEmpty(Entity))
                    Entity = "IMKE";

                var fileDate = DateTime.Now;

                var lookUp = new Dictionary<string, string>();

                var exemptAccs = new List<string>();

                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    if (!dbContext.Configurations.Any(c => c.Key == "GLExemptAccounts"))
                    {
                        dbContext.Configurations.Add(new Configuration
                        {
                            ConfigType = ConfigurationType.Account, Key = "GLExemptAccounts",
                            Value = "25049787002,25049787004,20100243506064"
                        });
                        await dbContext.SaveChangesAsync();
                    }

                    exemptAccs.AddRange(dbContext.Configurations.Where(c => c.Key == "GLExemptAccounts")
                        .FirstOrDefault()?.Value.Split(","));

                    var pairs = dbContext.Accounts.Select(a => new {a.Number, a.Name});

                    foreach (var acc in pairs) lookUp.TryAdd(acc.Number, acc.Name);
                }

                if (Path.GetExtension(filePath).ToLower() != ".csv")
                    return false;

                var output = new StringBuilder();
                //code to convert
                using (var reader = new StreamReader(filePath))
                {
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        while (csv.Read())
                        {
                            var accNo = csv.GetField(0);
                            var currency = csv.GetField(1);
                            var date1 = csv.GetField<DateTime>(2);
                            var DorC = csv.GetField<int>(3);
                            var openingBalance = csv.GetField<double>(4);
                            var date2 = fileDate = csv.GetField<DateTime>(5);
                            var DorC2 = csv.GetField<int>(6);
                            var closingBalance = csv.GetField<double>(7);

                            var multiplyBy = exemptAccs.Contains(accNo) ? 1 : -1;

                            if (filePath.ToLower().Contains("_sus") && Entity == "IMRW") multiplyBy = 1;

                            var toAppend =
                                $"{Entity}\t{accNo}\t{functionalArea}\t\t\t\t\t\t\t\t{GetAccountName(accNo, lookUp)}\t{functionalArea}\tA\tAsset\tTRUE\tTRUE\t\t{currency}\t{ContentHelpers.GetLastDayOfTheMonth(date2):MM/dd/yyyy}\t\t\t{multiplyBy * DorC2 * closingBalance}\n";

                            output.Append(toAppend);
                        }
                    }

                    reader.Close();
                }

                var outputPath = GetFileOutputName(filePath, fileDate);

                if (!File.Exists(outputPath))
                    await File.WriteAllTextAsync(outputPath, output.ToString());

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return false;
            }
        }

        private string GetFileOutputName(string filePath, DateTime fileDate)
        {
            var outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                $"GLAccounts_{fileDate:yyyyMMdd}_{Entity}.txt");

            if (filePath.ToLower().Contains("nostro"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_NOSTRO_{Entity}.txt");

            if (filePath.ToLower().Contains("bnr"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_BNR_{Entity}.txt");

            if (filePath.ToLower().Contains("bplus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_BPLUS_{Entity}.txt");

            if (filePath.ToLower().Contains("float"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_FLOAT_{Entity}.txt");

            if (filePath.ToLower().Contains("float") && filePath.ToLower().Contains("spenn"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_SFLOAT_{Entity}.txt");

            if (filePath.ToLower().Contains("selcom") && filePath.ToLower().Contains("spenn"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_SELCOM_{Entity}.txt");

            if (filePath.ToLower().Contains("mb"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_MB_{Entity}.txt");

            if (filePath.ToLower().Contains("b2w"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_B2W_{Entity}.txt");

            if (filePath.ToLower().Contains("w2b"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_W2B_{Entity}.txt");

            if (filePath.ToLower().Contains("util"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_UTIL_{Entity}.txt");
            if (filePath.ToLower().Contains("br_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_BR_SUS_{Entity}.txt");

            if (filePath.ToLower().Contains("fco_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_FCO_SUS_{Entity}.txt");

            if (filePath.ToLower().Contains("clearing"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_CLEAR_{Entity}.txt");

            if (filePath.ToLower().Contains("mg_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_MG_{Entity}.txt");

            if (filePath.ToLower().Contains("wu_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_WU_{Entity}.txt");

            if (filePath.ToLower().Contains("treasury_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TREASURY_{Entity}.txt");

            if (filePath.ToLower().Contains("fin_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_FIN_{Entity}.txt");

            if (filePath.ToLower().Contains("cre_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_CRE_{Entity}.txt");

            if (filePath.ToLower().Contains("ops_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_OPS_{Entity}.txt");

            return outputPath;
        }

        private string GetAccountName(string accountNumber, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(accountNumber))
                return dict[accountNumber];
            return accountNumber;
        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
    }
}