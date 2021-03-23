using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters
{
    public class BalanceFileConverter
    {
        private readonly static object _locker = new object();
        public ILogger Logger { get; set; }

        public Guid Id => new Guid("701d74d6-bb48-4384-9d73-1466de46e61f");

        public string Name => "Finacle Balance File Converter";

        public string Description => "Converts finacle generated csv file to standard blackline tab separated file";

        public string OutputFolder { get; set; }
        public int StartDelay { get; set; }
        public bool IsManualRun { get; set; }
        public string Entity { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public BalanceFileConverter(ILogger logger, IServiceScopeFactory serviceScopeFactory, string entity)
        {
            Logger = logger;
            ServiceScopeFactory = serviceScopeFactory;
            Entity = entity;
        }

        public async Task<bool> Execute(string filePath, string functionalArea = "Nostros")
        {
            try
            {
                if (string.IsNullOrEmpty(Entity))
                    Entity = "IMKE";

                DateTime fileDate = DateTime.Now;

                Dictionary<string, string> lookUp = new Dictionary<string, string>();

                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var pairs = dbContext.Accounts.Select(a => new { a.Number, a.Name });

                    foreach (var acc in pairs)
                    {
                        lookUp.TryAdd(acc.Number, acc.Name);
                    }
                }

                if (Path.GetExtension(filePath).ToLower() != ".csv")
                    return false;

                StringBuilder output = new StringBuilder();
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

                            var multiplyBy = accNo == "25049787002" || accNo == "25049787004" ? 1 : -1;

                            string toAppend = $"{Entity}\t{accNo}\t{functionalArea}\t\t\t\t\t\t\t\t{GetAccountName(accNo, lookUp)}\t{functionalArea}\tA\tAsset\tTRUE\tTRUE\t\t{currency}\t{ContentHelpers.GetLastDayOfTheMonth(date2):MM/dd/yyyy}\t\t\t{multiplyBy * DorC2 * closingBalance}\n";

                            output.Append(toAppend);
                        }

                    }
                    reader.Close();
                }

                var outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_{Entity}.txt");

                if (filePath.ToLower().Contains("bnr"))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_BNR_{Entity}.txt");
                }
                if (filePath.ToLower().Contains("b2w"))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_B2W_{Entity}.txt");
                }
                if (filePath.ToLower().Contains("w2b"))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_W2B_{Entity}.txt");
                }
                if (filePath.ToLower().Contains("selcom") && filePath.ToLower().Contains("spenn"))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_SELCOM_{Entity}.txt");
                }
                if (filePath.ToLower().Contains("float") && filePath.ToLower().Contains("spenn"))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_FLOAT_{Entity}.txt");
                }
                if (filePath.ToLower().Contains("mb") || filePath.ToLower().Contains("mb_util"))
                {
                    outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_MB_{Entity}.txt");
                }

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

        private string GetAccountName(string accountNumber, Dictionary<string, string> dict)
        {
            if (dict.ContainsKey(accountNumber))
            {
                return dict[accountNumber];
            }
            else
            {
                return accountNumber;
            }
        }

        private bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
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
