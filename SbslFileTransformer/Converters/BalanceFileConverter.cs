using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Data;
using SbslFileTransformer.Infrastructure.Helpers;
using SbslFileTransformer.Infrastructure.Messaging;
using SbslFileTransformer.Models;
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
            using (IServiceScope scope = ServiceScopeFactory.CreateScope())
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
                EmailSender emailSender = scope.ServiceProvider.GetService<EmailSender>();

                List<Configuration> configurations = await dbContext.Configurations.ToListAsync();

                try
                {
                    if (string.IsNullOrEmpty(Entity))
                        Entity = "IMKE";

                    DateTime fileDate = DateTime.Now;

                    Dictionary<string, string> lookUp = new Dictionary<string, string>();

                    List<string> exemptAccs = new List<string>();

                    string[] existingAccs = configurations.Where(c => c.Key == "GLExemptAccounts")
                        .FirstOrDefault()?.Value.Split(",");

                    if (existingAccs != null)
                        exemptAccs.AddRange(existingAccs);

                    var pairs = dbContext.Accounts.Select(a => new { a.Number, a.Name });

                    foreach (var acc in pairs)
                    {
                        lookUp.TryAdd(acc.Number, acc.Name);
                    }

                    if (Path.GetExtension(filePath).ToLower() != ".csv")
                        return false;

                    StringBuilder output = new StringBuilder();
                    //code to convert
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            while (csv.Read())
                            {
                                string accNo = csv.GetField(0);
                                string currency = csv.GetField(1);
                                DateTime date1 = csv.GetField<DateTime>(2);
                                int DorC = csv.GetField<int>(3);
                                double openingBalance = csv.GetField<double>(4);
                                DateTime date2 = fileDate = csv.GetField<DateTime>(5);
                                int DorC2 = csv.GetField<int>(6);
                                double closingBalance = csv.GetField<double>(7);

                                int multiplyBy = exemptAccs.Contains(accNo) ? 1 : -1;

                                if (filePath.ToLower().Contains("_sus") && Entity == "IMRW") 
                                    multiplyBy = 1;
                                if (filePath.ToLower().Contains("_sus") && Entity == "IMUG")
                                    multiplyBy = 1;
                                if (filePath.ToLower().Contains("_sus") && Entity == "IMUG" && DorC2==-1)
                                    multiplyBy = -1;
                                if (filePath.ToLower().Contains("wu_balances") && Entity == "IMUG" && DorC2 == 1)
                                    multiplyBy = 1;
                                if (filePath.ToLower().Contains("mg_balances") && Entity == "IMUG" && DorC2 == 1)
                                    multiplyBy = 1;
                                string toAppend =
                                    $"{Entity}\t{accNo}\t{functionalArea}\t\t\t\t\t\t\t\t{this.GetAccountName(accNo, lookUp)}\t{functionalArea}\tA\tAsset\tTRUE\tTRUE\t\t{currency}\t{ContentHelpers.GetLastDayOfTheMonth(date2):MM/dd/yyyy}\t\t\t{multiplyBy * DorC2 * closingBalance}\n";

                                output.Append(toAppend);
                            }
                        }

                        reader.Close();
                    }

                    string outputPath = this.GetFileOutputName(filePath, fileDate);

                    if (!File.Exists(outputPath))
                        await File.WriteAllTextAsync(outputPath, output.ToString());

                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);

                    await EmailHelpers.SendEmails(configurations, "Problem Converting CDM Balance files", $"\n\n {ex.Message}", new[] { filePath }, emailSender);

                    return false;
                }
            }
        }

        private string GetFileOutputName(string filePath, DateTime fileDate)
        {
            string subFileName = Path.GetFileName(filePath).Substring(0, 10);

            string outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                $"GLAccounts_{fileDate:yyyyMMdd}_{subFileName}_{Entity}.txt");

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
            {
                string curr = filePath.ToLower().Contains("lcy") ? "LCY" : "FCY";
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_CLEAR_{curr}_{Entity}.txt");
            }
            if (filePath.ToLower().Contains("mg_sus"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_MG_{Entity}.txt");

            if (filePath.ToLower().Contains("wu_sus") || filePath.ToLower().Contains("westernunion_balance"))
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

            if (filePath.ToLower().Contains("cards_kenya"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_CARDS_{Entity}.txt");

            if (filePath.ToLower().Contains("mobile_money"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_MBMKE_{Entity}.txt");

            if (filePath.ToLower().Contains("mobile_utility"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_MBUKE_{Entity}.txt");

            if (filePath.ToLower().Contains("branch_suspense"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_BRSUSKE_{Entity}.txt");

            if (filePath.ToLower().Contains("treasurybills") || filePath.ToLower().Contains("treasurybonds"))
            {
                string curr = filePath.ToLower().Contains("bonds") ? "TBonds" : "TBills";
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_{curr}_{Entity}.txt");
            }

            if (filePath.ToLower().Contains("tresuspense"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TRESUS_{Entity}.txt");

            if (filePath.ToLower().Contains("trepmoneymarket"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TREMONMK_{Entity}.txt");

            if (filePath.ToLower().Contains("trecontiliab"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TRECOLIAB_{Entity}.txt");

            if (filePath.ToLower().Contains("trecontiasset"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TRECOASS_{Entity}.txt");

            if (filePath.ToLower().Contains("treintexp"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TREINEXP_{Entity}.txt");

            if (filePath.ToLower().Contains("treintinc"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TREININC_{Entity}.txt");

            if (filePath.ToLower().Contains("treposition"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_TREPOS_{Entity}.txt");

            if(filePath.ToLower().Contains("pos_pay"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_POSPAY_{Entity}.txt");

            if (filePath.ToLower().Contains("spenn_micro"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_SPENN_{Entity}.txt");

            if (filePath.ToLower().Contains("ria_bal"))
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                    $"GLAccounts_{fileDate:yyyyMMdd}_RIA_{Entity}.txt");

            if (Path.GetFileName(filePath).ToLower().StartsWith("card"))
            {
                subFileName = Path.GetFileName(filePath).Substring(4, 10);
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                        $"GLAccounts_{fileDate:yyyyMMdd}_CARDS_{subFileName}_{Entity}.txt");
            }
            
            if (Path.GetFileName(filePath).ToLower().StartsWith("imug_mobile"))
            {
                subFileName = Path.GetFileName(filePath).Substring(4, 25);
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                        $"GLAccounts_{fileDate:yyyyMMdd}_MB_{subFileName}_{Entity}.txt");
            }

            //IMUG_WU_
            if (Path.GetFileName(filePath).ToLower().StartsWith("imug_wu"))
            {
                subFileName = Path.GetFileName(filePath).Substring(4, 25);
                outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                        $"GLAccounts_{fileDate:yyyyMMdd}_WU_{subFileName}_{Entity}.txt");
            }


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
