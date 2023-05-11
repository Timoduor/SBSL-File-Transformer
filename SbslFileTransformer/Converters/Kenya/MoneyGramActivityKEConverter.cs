using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Kenya
{
    public class MoneyGramActivityKEConverter
    {
        private readonly ILogger _logger;

        public MoneyGramActivityKEConverter(ILogger logger)
        {
            this._logger = logger;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int countHeader = 0;

                    double per = 0.2;

                    string excise = "Excise duty";

                    string computedbaseamnt = "Computed Base Amount";

                    while (reader.Read())
                    {
                        ExcelCols row = new ExcelCols();

                        string value = reader.GetValue(1)?.ToString();

                        if (string.IsNullOrEmpty(value) || value.Contains("Account Number : ") ||
                            value.Contains("Settlement Currency : ")) continue;

                        //tran date
                        row.Col0 = reader.GetValue(1)?.ToString().Replace("\n", "");
                        //tran id
                        row.Col1 = reader.GetValue(4)?.ToString().Replace("\n", "");
                        //ref #
                        row.Col2 = reader.GetValue(8)?.ToString().Replace("\n", "");
                        //prod
                        row.Col3 = reader.GetValue(11)?.ToString().Replace("\n", "");
                        //type
                        row.Col4 = reader.GetValue(12)?.ToString().Replace("\n", "");
                        //origin cntry
                        row.Col5 = reader.GetValue(14)?.ToString().Replace("\n", "");
                        //rev cntry
                        row.Col6 = reader.GetValue(15)?.ToString().Replace("\n", "");
                        //fx rate
                        row.Col7 = reader.GetValue(17)?.ToString().Replace("\n", "");
                        //fx date
                        row.Col8 = reader.GetValue(22)?.ToString().Replace("\n", "");
                        //fx margin
                        row.Col9 = reader.GetValue(23)?.ToString().Replace("\n", "");
                        //base amount
                        row.Col10 = reader.GetValue(25)?.ToString().Replace("\n", "");
                        //fee amount
                        row.Col11 = reader.GetValue(26)?.ToString().Replace("\n", "");
                        //fx rev share amount
                        row.Col12 = reader.GetValue(28)?.ToString().Replace("\n", "") +
                                    reader.GetValue(29)?.ToString().Replace("\n", "") +
                                    reader.GetValue(30)?.ToString().Replace("\n", "");
                        //commission amount
                        row.Col13 = reader.GetValue(33)?.ToString().Replace("\n", "") +
                                    reader.GetValue(34)?.ToString().Replace("\n", "");

                        if (countHeader == 3)
                        {
                            row.Col14 = excise;
                            row.Col15 = computedbaseamnt;
                        }

                        countHeader++;

                        try
                        {
                            double baseamnt = Convert.ToDouble(reader.GetValue(25));
                            double feeamnt = Convert.ToDouble(reader.GetValue(26));

                            if (reader.GetValue(11) != null && reader.GetValue(12) != null &&
                                reader.GetValue(11).ToString() == "MT" && reader.GetValue(12).ToString() == "SEN")
                                row.Col15 = (baseamnt + feeamnt + feeamnt * per).ToString();
                            else
                                row.Col15 = reader.GetValue(25)?.ToString();
                        }
                        catch (Exception)
                        {
                        }

                        list.Add(row);
                    }
                }
            }

            List<ExcelCols> list2 = this.ProduceSecondList(inputFile).Skip(1).ToList();

            List<ExcelCols> list3 = this.CombineTheTwoLists(list, list2);

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MG_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(list3, outputFile);
        }

        private List<ExcelCols> ProduceSecondList(string inputFile)
        {
            List<ExcelCols> list3 = new List<ExcelCols>();

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int countHeader1 = 0;

                    double per = 0.2;

                    while (reader.Read())
                    {
                        string excise = "Excise duty";

                        ExcelCols row = new ExcelCols();

                        string value1 = reader.GetValue(1)?.ToString();

                        if (string.IsNullOrEmpty(value1) || value1.Contains("Account Number : ") ||
                            value1.Contains("Settlement Currency : ")) continue;

                        string value2 = reader.GetValue(12)?.ToString();

                        if (string.IsNullOrEmpty(value2) || value2.Contains("REC")) continue;

                        //tran date
                        row.Col0 = reader.GetValue(1)?.ToString().Replace("\n", "");
                        //tran id
                        row.Col1 = reader.GetValue(4)?.ToString().Replace("\n", "");
                        //ref #
                        row.Col2 = reader.GetValue(8)?.ToString().Replace("\n", "");
                        //prod
                        row.Col3 = "excise duty";
                        //type
                        row.Col4 = reader.GetValue(12)?.ToString().Replace("\n", "");
                        //origin cntry
                        row.Col5 = reader.GetValue(14)?.ToString().Replace("\n", "");
                        //rev cntry
                        row.Col6 = reader.GetValue(15)?.ToString().Replace("\n", "");
                        //fx rate
                        row.Col7 = reader.GetValue(17)?.ToString().Replace("\n", "");
                        //fx date
                        row.Col8 = reader.GetValue(22)?.ToString().Replace("\n", "");
                        //fx margin
                        row.Col9 = reader.GetValue(23)?.ToString().Replace("\n", "");
                        //base amount
                        row.Col10 = reader.GetValue(25)?.ToString().Replace("\n", "");
                        //fee amount
                        row.Col11 = reader.GetValue(26)?.ToString().Replace("\n", "");
                        //fx rev share amount
                        row.Col12 = reader.GetValue(28)?.ToString().Replace("\n", "") +
                                    reader.GetValue(29)?.ToString().Replace("\n", "") +
                                    reader.GetValue(30)?.ToString().Replace("\n", "");
                        //commission amount
                        row.Col13 = reader.GetValue(33)?.ToString().Replace("\n", "") +
                                    reader.GetValue(34)?.ToString().Replace("\n", "");

                        //excise duty calculation (0.2% of amount)
                        try
                        {
                            double cost = Convert.ToDouble(reader.GetValue(26));
                            row.Col14 = (cost * per).ToString("0.##");
                        }
                        catch (Exception)
                        {
                        }

                        row.Col15 = row.Col14;

                        if (countHeader1 == 0)
                        {
                            row.Col14 = excise;
                            countHeader1++;
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