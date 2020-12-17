using CsvHelper;
using Microsoft.Extensions.Logging;
using PluginBase;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FinacleBalanceFileConverter
{
    public class BalanceFileConverter : RunnableBase
    {
        private readonly static object _locker = new object();
        public override ILogger<IRunnable> Logger { get; set; }

        public override Guid Id => new Guid("701d74d6-bb48-4384-9d73-1466de46e61f");

        public override string Name => "Finacle Balance File Converter";

        public override string Description => "Converts finacle generated csv file to standard blackline tab separated file";

        public override string OutputFolder { get; set; }
        public override int StartDelay { get; set; }
        public override bool IsManualRun { get; set; }
        public override string Entity { get; set; }

        public override async Task<bool> Execute(string filePath)
        {
            try
            {
                await base.Execute(filePath);

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
                                var date2 = csv.GetField<DateTime>(5);
                                var DorC2 = csv.GetField<int>(6);
                                var closingBalance = csv.GetField<double>(7);

                                string toAppend = $"{Entity}\t{accNo}\tNostros\t\t\t\t\t\t\t{GetAccountName(accNo)}\tNostros\tA\tAsset\tTRUE\tTRUE\t{currency}\t{date2.ToString("MM/dd/yyyy")}\t\t{-1 * DorC2 * closingBalance}\r";

                                output.AppendLine(toAppend);
                            }

                        }
                        reader.Close();
                    }

                    var outputPath = Path.Combine(Path.ChangeExtension(filePath, ".txt"));

                    if (!File.Exists(outputPath))
                    {
                        File.WriteAllText(outputPath, output.ToString());
                    }
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


        private string GetAccountName(string accountNumber)
        {
            string lookUp = @"19991211504015,STANDARD CHARTERED BANK DUBAI AED
19990911504013,STANDARD CHARTERED BANK -AUD
19990811504012,CITIBANK LONDON - CAD
19991111504014,HABIB AG ZURICH - CHF
19992011504016,STANDARD CHARTERED HONG KONG - CNY
19990611504008,COMMERZBANK AG
19990611504009,JP MORGAN AG FRANKFURT EUR
19990611504007,STANDARD CHARTERED BANK - FFT - EUR
19990511504004,CITIBANK LONDON - GBP
19990511504006,JP MORGAN LONDON - GBP
19990511504005,STANDARD CHARTERED BANK - LONDON- GBP
19991311504016,HDFC BANK - INR
19991311504017,ICICI BANK LTD - INR
19991311504012,YES BANK INDIA-INR
19990711504011,BANK OF TOKYO - JPY
19990711504010,STANDARD CHARTERED BANK - TOKYO - JPY
19990111501001,CO-OPERATIVE BANK LTD - KES
19991724051004,BANK ONE LTD - MUR
19990224051001,I&M BANK (RWANDA) LIMITED
19990324051003,I&M BANK (T) LTD - TZS
19991611504014,DFCU BANK UGANDA
19990424051004,BANK ONE LTD - USD
19990411504018,CITIBANK NEW YORK - USD
19990424051002,I&M BANK (T) LTD - USD
19990411504003,JP MORGAN NEW YORK - USD
19990411504001,STANDARD CHARTERED BANK - NY- USD
19990411504002,ICICI BANK HONG KONG (USD)
19991411504013,STANDARD BANK OF S.A - ZAR
19990110501001,CURRENT ACCOUNT WITH CBK - KES
19990510505002,CURRENT ACCOUNT WITH CBK - GBP
19990610505001,CURRENT ACCOUNT WITH CBK - EUR
19990410505006,CURRENT ACCOUNT WITH CBK - USD
19990310505004,CURRENT ACCOUNT WITH CENTRAL BANK-FCY-TZS
19991610505005,CURRENT ACCOUNT WITH CENTRAL BANK-FCY-UGX
19990210505003,CURRENT ACCOUNT WITH CENTRAL BANK-FCY-RWF
";
            string[] lines = lookUp.Split(new char[] { '\n', '\r' });

            var dict = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                var parts = line.Split(",");

                if (parts.Length == 2)
                {
                    dict.Add(parts[0], parts[1]);
                }
            }

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
