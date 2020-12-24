using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using SbslFileTransformer.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.PluginsLocal
{
    public class BalanceFileConverter : RunnableBase
    {
        private readonly static object _locker = new object();
        public override ILogger Logger { get; set; }

        public override Guid Id => new Guid("701d74d6-bb48-4384-9d73-1466de46e61f");

        public override string Name => "Finacle Balance File Converter";

        public override string Description => "Converts finacle generated csv file to standard blackline tab separated file";

        public override string OutputFolder { get; set; }
        public override int StartDelay { get; set; }
        public override bool IsManualRun { get; set; }
        public override string Entity { get; set; }
        public IServiceScopeFactory ServiceScopeFactory { get; set; }

        public BalanceFileConverter(ILogger logger, IServiceScopeFactory serviceScopeFactory, string entity)
        {
            Logger = logger;
            ServiceScopeFactory  = serviceScopeFactory;
            Entity = entity;
        }

        public override async Task<bool> Execute(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(Entity))
                    Entity = "IMKE";

                await base.Execute(filePath);

                DateTime fileDate = DateTime.Now;

                Dictionary<string, string> lookUp = new Dictionary<string, string>();

                using (var scope = ServiceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                    var pairs = dbContext.Accounts.Select(a => new { a.Number, a.Name });

                    foreach(var acc in pairs)
                    {
                        lookUp.TryAdd(acc.Number, acc.Name);
                    }
                }

                lock (_locker)
                {
                    if (Path.GetExtension(filePath) != ".csv")
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

                                string toAppend = $"{Entity}\t{accNo}\tNostros\t\t\t\t\t\t\t\t{GetAccountName(accNo, lookUp)}\tNostros\tA\tAsset\tTRUE\tTRUE\t\t{currency}\t{new DateTime(date2.Year, date2.Month, 1).AddMonths(1).AddDays(-1):MM/dd/yyyy}\t\t\t{-1 * DorC2 * closingBalance}\n";

                                output.Append(toAppend);
                            }

                        }
                        reader.Close();
                    }

                    var outputPath = Path.Combine(Path.GetDirectoryName(filePath), $"GLAccounts_{fileDate:yyyyMMdd}_{fileDate:MMddyyyy}.txt");

                    File.WriteAllText(outputPath, output.ToString());

                    //File.Delete(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return false;
            }
        }


        private string GetAccountName(string accountNumber, Dictionary<string,string> dict)
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

        public override void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
