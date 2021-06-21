using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class FxRatesTZConverter
    {
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var tables = result.Tables;

                    var sheet1 = tables[0];

                    int countHeader = 0;

                    string dateValue = "";

                    string rateValue = "null";

                    string date = "Date";

                    string currency = "Currency";

                    string rateType = "Rate_type";

                    string sendPay_Indicator = "SendPay_Indicator";

                    string rate = "Rate";


                    if (countHeader == 0)
                    {
                        var header = new ExcelCols();

                        header.Col0 = date;

                        header.Col1 = currency;

                        header.Col2 = rateType;

                        header.Col3 = sendPay_Indicator;

                        header.Col4 = rate;

                        list.Add(header);

                        countHeader++;
                    }


                    foreach (DataRow row in sheet1.Rows)
                    {
                        var column1 = row[1]?.ToString();

                        var column2 = row[2]?.ToString();

                        if (column2 != null && column2.Contains("/"))
                        {
                            dateValue = column2.Substring(0, 9);
                        }

                        if (column1 != null && column1.Contains("CASH RATES"))
                        {
                            rateValue = column1;
                        }

                        if (rateValue.Equals("CASH RATES") && column1 != null && column1.Contains("USD/TZS"))
                        {
                            var buyingRate = new ExcelCols();

                            buyingRate.Col0 = dateValue.Replace(",", "");

                            buyingRate.Col1 = column1.Split('/')[0].Replace(",", "");

                            buyingRate.Col2 = "Buying Rate".Replace(",", "");

                            buyingRate.Col3 = "S".Replace(",", "");

                            buyingRate.Col4 = row[2].ToString().Replace(",", "");


                            var sellingRate = new ExcelCols();

                            sellingRate.Col0 = dateValue.Replace(",", "");

                            sellingRate.Col1 = column1.Split('/')[0].Replace(",", "");

                            sellingRate.Col2 = "Selling Rate".Replace(",", "");

                            sellingRate.Col3 = "P".Replace(",", "");

                            sellingRate.Col4 = row[3].ToString().Replace(",", "");

                            list.Add(buyingRate);

                            list.Add(sellingRate);

                            rateValue = "null";
                        }

                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);
                var fileNameToUse = fileName.Replace(" ", "").Substring(Math.Max(0, fileName.Length - 15));

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_FxRs_{fileNameToUse}.csv");
            }

            WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            //foreach (var item in rows)
            //{
            //    File.WriteAllLines(item);
            //}

            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {

                    foreach (var row in rows)
                    {
                        //csv.Configuration.Delimiter = "\t";
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}
