using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class FxposftsumConverter
    {
        public FxposftsumConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        public static DateTime GetLastDayOfTheMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        public void ConvertFile(string inputFile, string outputFolder = "")
        {
            List<ExcelCols> list = new List<ExcelCols>();
            string outputFile = "";
            //string outputFolder = null;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
                //outputFile = outputFile + "\\conv";

                if (!Directory.Exists(outputFile))
                {
                    Directory.CreateDirectory(outputFile);
                }

            }

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        ExcelCols row = new ExcelCols();

                        string value = reader.GetValue(1)?.ToString();

                        string value1 = reader.GetValue(2)?.ToString();

                        if (value.Contains("Currency Desc") || value.Contains("Net Open Position Equivalent") || value.Contains("Today's Customer FX P&L") || value.Contains("Customer FX P&L for 02-Aug-2021")
                             || value.Contains("Customer FX P&L Month to Date") || value.Contains("Customer FX P&L Year to Date") || value.Contains("Reval P&L for 02-Aug-2021") || value.Contains("Total P&L for 02-Aug-2021")
                             || value.Contains("Total P&L Month to Date") || value.Contains("Total P&L Year to Date") || value.Contains("NOP to Core Capital Ratio (Internal)"))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(value1) || value1.Contains("FOREX POSITION PAD"))
                        {
                            continue;
                        }
                        row.Col0 = "IMRW";

                        row.Col11 = "Balance_bank";

                        //date column
                        row.Col12 = GetLastDayOfTheMonth(DateTime.Now).ToString("M/dd/yyyy");

                        row.Col16 = reader.GetValue(5)?.ToString();

                        row.Col17 = reader.GetValue(2)?.ToString().Replace("\n", "");

                        if (reader.GetValue(2).ToString().Contains("CAD"))
                        {
                            row.Col1 = "20980882501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("EUR"))
                        {
                            row.Col1 = "20980682501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("INR"))
                        {
                            row.Col1 = "20981382501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("JPY"))
                        {
                            row.Col1 = "20980782501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("KES"))
                        {
                            row.Col1 = "20980182501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("GBP"))
                        {
                            row.Col1 = "20980582501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("RWF"))
                        {
                            row.Col1 = "20980282501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("ZAR"))
                        {
                            row.Col1 = "20981482501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("CHF"))
                        {
                            row.Col1 = "20981182501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("TZS"))
                        {
                            row.Col1 = "20980382501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("USD"))
                        {
                            row.Col1 = "20980482501021";
                        }
                        if (reader.GetValue(2).ToString().Contains("UGX"))
                        {
                            row.Col1 = "20981682501021";
                        }

                        list.Add(row);
                    }
                }
            }
            outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{Path.GetFileNameWithoutExtension(inputFile)}_fxpostsum_{"IMRW"}.txt");
            //outputFile = outputFile + "\\Converted_" + Path.GetFileNameWithoutExtension(inputFile) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".txt";
            WriteToFile(list, outputFile);
        }


        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t"
                };

                using (CsvWriter csv = new CsvWriter(writer, config))
                {
                    foreach (ExcelCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
        public class ExcelCols
        {
            public string Col0 { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
            public string Col3 { get; set; }
            public string Col4 { get; set; }
            public string Col5 { get; set; }
            public string Col6 { get; set; }
            public string Col7 { get; set; }
            public string Col8 { get; set; }
            public string Col9 { get; set; }
            public string Col10 { get; set; }
            public string Col11 { get; set; }
            public string Col12 { get; set; }
            public string Col13 { get; set; }
            public string Col14 { get; set; }
            public string Col15 { get; set; }
            public string Col16 { get; set; }
            public string Col17 { get; set; }

        }
    }
}
