using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.Kenya
{
    public class MoneyGramSettlementRWConverter
    {
        public MoneyGramSettlementRWConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var countHeader = 0;

                    var count = 0;

                    var per = 0.18;

                    var ch = 0.78;

                    var accountNo = "Account Number";

                    var agentName = "Agent Name";

                    var vat = "VAT";

                    var mk = "MK";

                    var computedbaseamnt = "Computed Base Amount";

                    var charge = "Charge";

                    var revenue = "Revenue";

                    var amountFinal = "Amount Final";

                    var bankCode = "";

                    var agentDetails = "";

                    while (reader.Read())
                    {
                        count++;

                        var row = new ExcelCols();

                        var accountValue = reader.GetValue(1)?.ToString();

                        var agentValue = reader.GetValue(4)?.ToString();

                        if (!string.IsNullOrEmpty(accountValue))
                        {
                            if (accountValue.Contains("Settlement Currency") ||
                                accountValue.Contains("Settlement Id") ||
                                accountValue.Contains("Business Date"))
                                continue;
                        }
                        else if (!string.IsNullOrEmpty(reader.GetValue(10)?.ToString()) &&
                                 reader.GetValue(10).ToString().Contains("Tran"))
                        {
                            continue;
                        }


                        if (!string.IsNullOrEmpty(accountValue) && accountValue.Contains("Account Number"))
                        {
                            var rec = reader.GetValue(5)?.ToString();

                            bankCode = rec;
                        }
                        else
                        {
                            if (countHeader == 3)
                            {
                                row.Col0 = "Account Number";

                                row.Col1 = agentName;

                                row.Col16 = mk;

                                row.Col17 = vat;

                                row.Col19 = computedbaseamnt;

                                row.Col20 = charge;

                                row.Col21 = revenue;

                                row.Col22 = amountFinal;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(agentValue))
                                    if (agentValue.Contains("Agent Name"))
                                        agentDetails = reader.GetValue(8)?.ToString();
                                row.Col0 = bankCode;

                                row.Col1 = agentDetails;
                            }

                            //tran date
                            row.Col2 = reader.GetValue(1)?.ToString().Replace("\n", "");
                            //tran id
                            row.Col3 = reader.GetValue(3)?.ToString().Replace("\n", "");
                            //ref #
                            row.Col4 = reader.GetValue(7)?.ToString().Replace("\n", "");
                            //prod
                            row.Col5 = reader.GetValue(9)?.ToString().Replace("\n", "");
                            //tran type
                            row.Col6 = reader.GetValue(10)?.ToString().Replace("\n", "");
                            //origin cntry
                            row.Col7 = reader.GetValue(11)?.ToString().Replace("\n", "");
                            //rev cntry
                            row.Col8 = reader.GetValue(13)?.ToString().Replace("\n", "");
                            //fx rate
                            row.Col9 = reader.GetValue(15)?.ToString().Replace("\n", "");
                            //fx date
                            row.Col10 = reader.GetValue(19)?.ToString().Replace("\n", "");
                            //fx margin
                            row.Col11 = reader.GetValue(20)?.ToString().Replace("\n", "");
                            //base amount
                            row.Col12 = reader.GetValue(23)?.ToString().Replace("\n", "");
                            //fee amount
                            row.Col13 = reader.GetValue(24)?.ToString().Replace("\n", "") +
                                        reader.GetValue(25)?.ToString().Replace("\n", "");
                            //fx rev share amount
                            row.Col14 = reader.GetValue(28)?.ToString().Replace("\n", "") +
                                        reader.GetValue(29)?.ToString().Replace("\n", "");
                            //commission amount
                            row.Col15 = reader.GetValue(32)?.ToString().Replace("\n", "") +
                                        reader.GetValue(33)?.ToString().Replace("\n", "");
                            //mk
                            if (reader.GetValue(34) != null)
                                row.Col16 = reader.GetValue(34)?.ToString().Replace("\n", "");
                            //total
                            row.Col18 = reader.GetValue(35)?.ToString().Replace("\n", "");
                        }

                        countHeader++;

                        try
                        {
                            var baseamnt = Convert.ToDouble(reader.GetValue(23));
                            var feeamnt = Convert.ToDouble(reader.GetValue(24));

                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "SEN")
                            {
                                //VAT
                                row.Col17 = (feeamnt * per).ToString();

                                row.Col19 = Math.Ceiling(baseamnt + feeamnt + feeamnt * per).ToString();
                            }

                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "REC")
                                row.Col19 = Math.Truncate(baseamnt).ToString();
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "SEN" &&
                                reader.GetValue(34) != null &&
                                reader.GetValue(34).ToString() == "mk")
                                row.Col19 = reader.GetValue(32)?.ToString().Replace("\n", "");
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "REF")
                                row.Col19 = reader.GetValue(32)?.ToString().Replace("\n", "");
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "SEN")
                                //charge
                                row.Col20 = (feeamnt * ch).ToString();
                        }
                        catch (Exception)
                        {
                        }

                        if (!string.IsNullOrEmpty(row.Col2))
                            if (row.Col2.Contains("/") || row.Col2.Contains("Tran Date")
                                                       || row.Col2.Contains("RW") || row.Col2.Contains("KIGALI")
                                                       || row.Col2.Contains("3 AVENUE KN 9"))
                                list.Add(row);
                    }
                }
            }

            var finalList = new List<ExcelCols>();
            finalList.Add(list[0]);
            var rev1 = 0.4;
            var rev2 = 0.5;
            var amntfinal = 0.022;

            foreach (var rows in list)
                try
                {
                    if (rows.Col1.Contains("COPEDU") || rows.Col1.Contains("GOSHEN"))
                        //revenue
                        rows.Col21 = ((Convert.ToDouble(rows.Col13) - Convert.ToDouble(rows.Col20)) * rev2).ToString();
                    else
                        //revenue
                        rows.Col21 = ((Convert.ToDouble(rows.Col13) - Convert.ToDouble(rows.Col20)) * rev1).ToString();
                    if (rows.Col6 == "SEN")
                        //amount final
                        rows.Col22 = Math.Ceiling(Convert.ToDouble(rows.Col12) + Convert.ToDouble(rows.Col20) +
                                                  Convert.ToDouble(rows.Col21) +
                                                  Convert.ToDouble(rows.Col13) * amntfinal).ToString();
                    if (rows.Col6 == "REF" || rows.Col6 == "REC" || rows.Col1 == "COPEDU" || rows.Col1 == "GOSHEN" ||
                        rows.Col1 == "RIM LTD" || rows.Col1 == "EXTRACASH LTD")
                        //amount final
                        rows.Col22 = Math.Floor(Convert.ToDouble(rows.Col12)).ToString();

                    finalList.Add(rows);
                }
                catch (Exception)
                {
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