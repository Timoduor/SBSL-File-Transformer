using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Kenya
{
    public class WesternUnionActivitiesKEConverter
    {
        public WesternUnionActivitiesKEConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream,
                    new ExcelReaderConfiguration { AutodetectSeparators = new[] { ',', ';', '\t', '|', '#' } }))
                {
                    int countHeader = 0;

                    double per = 0.2;

                    string excise = "Excise duty";

                    string computedbaseamnt = "Computed Base Amount";

                    int count = 0;

                    while (reader.Read())
                    {
                        count++;

                        ExcelCols row = new ExcelCols();

                        string value = reader.GetValue(0).ToString();

                        string check = reader.GetValue(1)?.ToString().Trim();

                        if (string.IsNullOrEmpty(check)) break;

                        if (value != "KES" && value != "Code" && list.Count() > 0)
                        {
                            ExcelCols last = list.Last();

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
                                last.Col19 = (recamnt + totalchamnt + totalchamnt * per).ToString().TrimStart()
                                    .TrimEnd();
                            else if (reader.GetValue(4).ToString().Contains("P"))
                                last.Col19 = reader.GetValue(17).ToString().TrimStart().TrimEnd();

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
                        }

                        countHeader++;

                        try
                        {
                            double recamnt = Convert.ToDouble(reader.GetValue(13));

                            double totalchamnt = Convert.ToDouble(reader.GetValue(14));

                            if (reader.GetValue(6).ToString().Contains("S"))
                                row.Col19 = (recamnt + totalchamnt + totalchamnt * per).ToString().TrimStart()
                                    .TrimEnd();
                            else if (reader.GetValue(6).ToString().Contains("P"))
                                row.Col19 = reader.GetValue(17)?.ToString().TrimStart().TrimEnd();
                        }
                        catch (Exception)
                        {
                        }

                        list.Add(row);
                    }
                }
            }

            List<ExcelCols> list2 = this.ProduceSecondList(inputFile).Skip(1).ToList();

            List<ExcelCols> list4 = this.CombineTheTwoLists(list, list2);

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_WUAKE_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(list4, outputFile);
        }

        private List<ExcelCols> ProduceSecondList(string inputFile)
        {
            List<ExcelCols> list3 = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateCsvReader(stream,
                    new ExcelReaderConfiguration { AutodetectSeparators = new[] { ',', ';', '\t', '|', '#' } }))
                {
                    double per = 0.2;

                    while (reader.Read())
                    {
                        ExcelCols row = new ExcelCols();

                        string value1 = reader.GetValue(0).ToString();

                        string check = reader.GetValue(1)?.ToString().Trim();

                        if (string.IsNullOrEmpty(check)) break;

                        //traversing through row 5
                        if (value1 != "KES" && value1 != "Code" && list3.Count() > 0)
                        {
                            string value3 = reader.GetValue(4)?.ToString();

                            ExcelCols last = list3.Last();

                            if (value3.Contains("S"))
                                last.Col6 = "excise duty";
                            else if (value3.Contains("P")) continue;

                            last.Col3 = reader.GetValue(1)?.ToString();

                            last.Col4 = reader.GetValue(2)?.ToString();

                            last.Col5 = reader.GetValue(3)?.ToString();

                            //last.Col6 = reader.GetValue(4)?.ToString();

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

                            try
                            {
                                double cost = Convert.ToDouble(last.Col14);

                                last.Col18 = (cost * per).ToString("0.##").TrimStart().TrimEnd();

                                last.Col19 = last.Col18.TrimStart().TrimEnd();
                            }
                            catch (Exception)
                            {
                            }

                            continue;
                        }

                        string value2 = reader.GetValue(6)?.ToString();

                        if (value2.Contains("S"))
                            row.Col6 = "excise duty";
                        else if (value2.Contains("P")) continue;

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
                        //row.Col6 = reader.GetValue(6)?.ToString().Trim().Replace("\n", "");
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

                        //excise duty calculation (0.2% of amount)
                        try
                        {
                            double cost = Convert.ToDouble(reader.GetValue(14));

                            row.Col18 = (cost * per).ToString("0.##").TrimStart().TrimEnd();

                            row.Col19 = row.Col18.TrimStart().TrimEnd();
                        }
                        catch (Exception)
                        {
                        }

                        list3.Add(row);
                    }
                }
            }

            return list3;
        }

        private List<ExcelCols> CombineTheTwoLists(List<ExcelCols> list, List<ExcelCols> list2)
        {
            List<ExcelCols> combinedList = new List<ExcelCols>();

            combinedList.AddRange(list);
            combinedList.AddRange(list2);

            return combinedList;
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