using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class WesternUnionActivitiesRWConverter
    {
        public WesternUnionActivitiesRWConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream, new ExcelReaderConfiguration() { AutodetectSeparators = new char[] { ',', ';', '\t', '|', '#' } }))
                {
                    int countHeader = 0;

                    double per = 0.18;

                    string excise = "VAT";

                    string computedbaseamnt = "Computed Base Amount";

                    string computedbaseamnt1 = "Computed Base Amount without decimal";

                    int count = 0;

                    while (reader.Read())
                    {
                        count++;

                        var row = new ExcelCols();

                        var value = reader.GetValue(0).ToString();

                        var check = reader.GetValue(1)?.ToString().Trim();

                        if (string.IsNullOrEmpty(check))
                        {
                            break;
                        }

                        if (value == null && list.Count() > 0)
                        {
                            var last = list.Last();

                            last.Col3 = reader.GetValue(1)?.ToString().Trim();

                            last.Col4 = reader.GetValue(2)?.ToString();

                            last.Col5 = reader.GetValue(3)?.ToString();

                            last.Col6 = reader.GetValue(4)?.ToString();

                            last.Col7 = reader.GetValue(5)?.ToString();

                            last.Col8 = reader.GetValue(6)?.ToString();

                            last.Col9 = reader.GetValue(7)?.ToString();

                            last.Col10 = reader.GetValue(8)?.ToString();

                            last.Col11 = reader.GetValue(9)?.ToString();

                            last.Col12 = reader.GetValue(10)?.ToString();

                            last.Col13 = reader.GetValue(11)?.ToString();

                            last.Col14 = reader.GetValue(12)?.ToString();

                            last.Col15 = reader.GetValue(13)?.ToString();

                            last.Col16 = reader.GetValue(14)?.ToString();

                            last.Col17 = reader.GetValue(15)?.ToString();

                            double recamnt = Convert.ToDouble(reader.GetValue(11));

                            double totalchamnt = Convert.ToDouble(reader.GetValue(12));

                            if (reader.GetValue(4).ToString().Contains("S"))
                            {
                                last.Col19 = (recamnt + totalchamnt + (totalchamnt * per)).ToString().TrimStart().TrimEnd();
                            }
                            else if (reader.GetValue(4).ToString().Contains("P"))
                            {
                                last.Col19 = reader.GetValue(17).ToString().TrimStart().TrimEnd();
                            }

                            continue;
                        }

                        //code
                        row.Col0 = reader.GetValue(0)?.ToString().Trim().Replace("\n", "");
                        //Location_ID
                        row.Col1 = reader.GetValue(1)?.ToString().Trim().Replace("\n", "");
                        //Name
                        row.Col2 = reader.GetValue(2)?.ToString().Trim().Replace("\n", "");
                        //Account
                        row.Col3 = reader.GetValue(3)?.ToString().Trim().Replace("\n", "");
                        //MTCN
                        row.Col4 = reader.GetValue(4)?.ToString().Trim().Replace("\n", "");
                        //Status
                        row.Col5 = reader.GetValue(5)?.ToString().Trim().Replace("\n", "");
                        //SendPayIndicator
                        row.Col6 = reader.GetValue(6)?.ToString().Trim().Replace("\n", "");
                        //txnDateUS
                        row.Col7 = reader.GetValue(7)?.ToString().Trim().Replace("\n", "");
                        //TxnDateYear
                        row.Col8 = reader.GetValue(8)?.ToString().Trim().Replace("\n", "");
                        //TxnDateMonth
                        row.Col9 = reader.GetValue(9)?.ToString().Trim().Replace("\n", "");
                        //TxnDateDay
                        row.Col10 = reader.GetValue(10)?.ToString().Trim().Replace("\n", "");
                        //TerminalID
                        row.Col11 = reader.GetValue(11)?.ToString().Trim().Replace("\n", "");
                        //OperatorID
                        row.Col12 = reader.GetValue(12)?.ToString().Trim().Replace("\n", "");
                        //RecPrincipalREC
                        row.Col13 = reader.GetValue(13)?.ToString().Trim().Replace("\n", "");
                        //TotalChargesREC
                        row.Col14 = reader.GetValue(14)?.ToString().Trim().Replace("\n", "");
                        //TaxesREC
                        row.Col15 = reader.GetValue(15)?.ToString().Trim().Replace("\n", "");
                        //TaxesPAY
                        row.Col16 = reader.GetValue(16)?.ToString().Trim().Replace("\n", "");
                        //PayPrincipalPAY
                        row.Col17 = reader.GetValue(17)?.ToString().Trim().Replace("\n", "");

                        if (countHeader == 0)
                        {
                            row.Col18 = excise;
                            row.Col19 = computedbaseamnt;
                            row.Col20 = computedbaseamnt1;
                        }

                        countHeader++;

                        try
                        {
                            double recamnt = Math.Ceiling(Convert.ToDouble(reader.GetValue(13)));

                            double totalchamnt = Convert.ToDouble(reader.GetValue(14));

                            double calcvat = totalchamnt * per;

                            double computedbase1 = Convert.ToDouble(reader.GetValue(17));

                            if (reader.GetValue(6).ToString().Contains("S"))
                            {
                                row.Col18 = Math.Round(calcvat).ToString();
                            }
                            if (reader.GetValue(6).ToString().Contains("S"))
                            {
                                row.Col19 = Math.Round(recamnt + totalchamnt + calcvat).ToString().TrimStart().TrimEnd();
                            }
                            if (reader.GetValue(6).ToString().Contains("S"))
                            {
                                row.Col20 = Math.Round(recamnt + totalchamnt + calcvat).ToString().TrimStart().TrimEnd();
                            }
                            if (reader.GetValue(6).ToString().Contains("P"))
                            {
                                row.Col19 = reader.GetValue(17).ToString().TrimStart().TrimEnd();
                            }
                            if (reader.GetValue(6).ToString().Contains("P"))
                            {
                                row.Col20 = Math.Truncate(computedbase1).ToString().TrimStart().TrimEnd();
                            }

                        }
                        catch (Exception)
                        {

                        }

                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_WUARW_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (ExcelCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}