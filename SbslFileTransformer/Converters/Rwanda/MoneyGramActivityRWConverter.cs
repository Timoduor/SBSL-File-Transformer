using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class MoneyGramActivityRWConverter
    {
        private ILogger _logger;

        public MoneyGramActivityRWConverter(ILogger logger)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _logger = logger;
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int countHeader = 0;

                    int count = 0;

                    double per = 0.18;

                    double ch = 0.78;

                    string accountNo = "Account Number";

                    string accountName = "Account Name";

                    string excise = "VAT";

                    string mk = "MK";

                    string computedbaseamnt = "Computed Base Amount";

                    string charge = "Charge";

                    string revenue = "Revenue";

                    string amountFinal = "Amount Final";

                    string bankCode = "";

                    string bankName = "";

                    string tran = "";

                    string set = "";

                    string settelementCurrency = "Settlement Currency";

                    string transactionCurrency = "Transaction Currency";

                    while (reader.Read())
                    {
                        count++;

                        var row = new ExcelCols();

                        //settelement currency
                        var value = reader.GetValue(1)?.ToString();

                        //transaction currency
                        var valueTran = reader.GetValue(10)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }
                        else if (value.Contains("Settlement Currency"))
                        {
                            set = reader.GetValue(7)?.ToString();
                        }
                        else if (value.Contains("Account Number"))
                        {
                            string rec = reader.GetValue(6)?.ToString();

                            bankCode = rec.Split(' ')[0];

                            bankName = rec.Replace(bankCode, "");
                        }
                        if (valueTran != null)
                        {
                            if (valueTran.Contains("Transaction Currency"))
                            {
                                tran = reader.GetValue(17)?.ToString();
                            }
                        }

                        if (countHeader == 3)
                        {
                            row.Col0 = accountNo;

                            row.Col1 = accountName;

                            row.Col16 = mk;

                            row.Col17 = excise;

                            row.Col18 = computedbaseamnt;

                            row.Col19 = charge;

                            row.Col20 = revenue;

                            row.Col21 = amountFinal;

                            row.Col22 = settelementCurrency;

                            row.Col23 = transactionCurrency;

                        }
                        else
                        {
                            row.Col0 = bankCode;

                            row.Col1 = bankName;

                            row.Col22 = set;

                            row.Col23 = tran;
                        }
                        //tran date
                        row.Col2 = reader.GetValue(1)?.ToString().Replace("\n", "");
                        //tran id
                        row.Col3 = reader.GetValue(4)?.ToString().Replace("\n", "");
                        //ref #
                        row.Col4 = reader.GetValue(8)?.ToString().Replace("\n", "");
                        //prod
                        row.Col5 = reader.GetValue(11)?.ToString().Replace("\n", "");
                        //type
                        row.Col6 = reader.GetValue(12)?.ToString().Replace("\n", "");
                        //origin cntry
                        row.Col7 = reader.GetValue(14)?.ToString().Replace("\n", "");
                        //rev cntry
                        row.Col8 = reader.GetValue(15)?.ToString().Replace("\n", "");
                        //fx rate
                        row.Col9 = reader.GetValue(17)?.ToString().Replace("\n", "");
                        //fx date
                        row.Col10 = reader.GetValue(22)?.ToString().Replace("\n", "");
                        //fx margin
                        row.Col11 = reader.GetValue(23)?.ToString().Replace("\n", "");
                        //base amount 
                        row.Col12 = reader.GetValue(25)?.ToString().Replace("\n", "");
                        //fee amount
                        row.Col13 = reader.GetValue(26)?.ToString().Replace("\n", "");
                        //fx rev share amount
                        row.Col14 = reader.GetValue(28)?.ToString().Replace("\n", "") + reader.GetValue(29)?.ToString().Replace("\n", "") + reader.GetValue(30)?.ToString().Replace("\n", "");
                        //commission amount
                        row.Col15 = reader.GetValue(33)?.ToString().Replace("\n", "") + reader.GetValue(34)?.ToString().Replace("\n", "");
                        //mk
                        if (reader.GetValue(35) != null)
                        {
                            row.Col16 = reader.GetValue(35)?.ToString().Replace("\n", "");
                        }
                        countHeader++;
                        try
                        {
                            double baseamnt = Convert.ToDouble(reader.GetValue(25));
                            double feeamnt = Convert.ToDouble(reader.GetValue(26));

                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "SEN")
                            {
                                row.Col17 = (feeamnt * per).ToString();

                                //computed base amount
                                row.Col18 = Math.Ceiling(baseamnt + feeamnt + (feeamnt * per)).ToString();
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "RSN")
                            {
                                row.Col17 = (feeamnt * per).ToString();

                                //computed base amount
                                row.Col18 = Math.Ceiling(baseamnt + feeamnt + (feeamnt * per)).ToString();
                                //row.Col18 = Math.Round(baseamnt + feeamnt + (feeamnt * per),MidpointRounding.AwayFromZero).ToString();
                                //row.Col18 = Math.Truncate(baseamnt + feeamnt + (feeamnt * per)).ToString();
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "REC")
                            {
                                //computed base amount
                                //row.Col18 = Math.Round(baseamnt,MidpointRounding.AwayFromZero).ToString();
                                row.Col18 = Math.Truncate(baseamnt).ToString();
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "REF")
                            {
                                //row.Col18 = Math.Round(baseamnt,MidpointRounding.AwayFromZero).ToString();
                                row.Col18 = Math.Truncate(baseamnt).ToString();
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "RDT")
                            {
                                row.Col18 = Math.Truncate(baseamnt).ToString();
                            }
                            if (reader.GetValue(35) != null && reader.GetValue(35).ToString() == "MK")
                            {
                                row.Col18 = reader.GetValue(33)?.ToString().Replace("\n", "");
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "REF")
                            {
                                row.Col18 = reader.GetValue(33)?.ToString().Replace("\n", "");
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "SEN")
                            {
                                //charge
                                row.Col19 = (feeamnt * ch).ToString();
                            }
                            if (reader.GetValue(12) != null && reader.GetValue(12).ToString() == "RSN")
                            {
                                //charge
                                row.Col19 = (feeamnt * ch).ToString();
                            }
                        }
                        catch (Exception)
                        {

                        }
                        if (string.IsNullOrEmpty(row.Col2))
                        {
                            continue;
                        }


                        list.Add(row);
                    }
                }
            }
            var finalList = new List<ExcelCols>();
            finalList.Add(list[4]);
            finalList[0].Col0 = list[3].Col0;
            finalList[0].Col1 = list[3].Col1;
            finalList[0].Col16 = list[3].Col16;
            finalList[0].Col17 = list[3].Col17;
            finalList[0].Col18 = list[3].Col18;
            finalList[0].Col19 = list[3].Col19;
            finalList[0].Col20 = list[3].Col20;
            finalList[0].Col21 = list[3].Col21;
            finalList[0].Col22 = list[3].Col22;
            finalList[0].Col23 = list[3].Col23;

            double rev1 = 0.4;
            double rev2 = 0.5;
            double amntfinal = 0.022;
            foreach (var rows in list)
            {
                try
                {
                    if (rows.Col6 == null)
                    {
                        continue;
                    }
                    if (rows.Col1.Contains("COPEDU") || rows.Col1.Contains("GOSHEN"))
                    {
                        //revenue
                        rows.Col20 = ((Convert.ToDouble(rows.Col13) - Convert.ToDouble(rows.Col19)) * rev2).ToString();
                    }
                    else
                    {
                        //revenue
                        rows.Col20 = ((Convert.ToDouble(rows.Col13) - Convert.ToDouble(rows.Col19)) * rev1).ToString();
                    }

                    if (rows.Col6.Contains("SEN"))
                    {
                        //amount final
                        rows.Col21 = Math.Ceiling(Convert.ToDouble(rows.Col12) + Convert.ToDouble(rows.Col19) + Convert.ToDouble(rows.Col20) + (Convert.ToDouble(rows.Col13) * amntfinal)).ToString();
                    }
                    else
                    {
                        //amount final
                        //double rev3 = Convert.ToDouble(rows.Col12.ToString());
                        rows.Col21 = Math.Floor(Convert.ToDouble(rows.Col12)).ToString();
                    }
                    if (rows.Col6.Contains("RSN"))
                    {
                        //amount final
                        rows.Col21 = Math.Ceiling(Convert.ToDouble(rows.Col12) + Convert.ToDouble(rows.Col19) + Convert.ToDouble(rows.Col20) + (Convert.ToDouble(rows.Col13) * amntfinal)).ToString();
                    }
                    else
                    {
                        //amount final
                        //double rev3 = Convert.ToDouble(rows.Col12.ToString());
                        rows.Col21 = Math.Floor(Convert.ToDouble(rows.Col12)).ToString();
                    }

                    finalList.Add(rows);
                }
                catch (Exception)
                {

                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MG_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}