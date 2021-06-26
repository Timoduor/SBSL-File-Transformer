using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;

namespace SbslFileTransformer.Converters.Kenya
{
    public class AdviceKopeduRWConverter
    {
        public AdviceKopeduRWConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();
            int count = 0;
            string IOBound = "";
            string location = "";
            string date = "";


            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        var index20 = reader.GetValue(20)?.ToString();
                        if (reader.GetValue(0) == null && reader.GetValue(1) == null && reader.GetValue(2) == null
                            && reader.GetValue(3) == null && reader.GetValue(4) == null && reader.GetValue(5) == null
                            && reader.GetValue(6) == null && reader.GetValue(7) == null
                            && reader.GetValue(8) == null && index20 == null)
                        {
                            continue;
                        }
                        else if (index20 != null && index20.Contains("Total"))
                        {
                            continue;
                        }

                        //set the headers
                        if (count == 0)
                        {

                            row.Col0 = "Date";
                            row.Col1 = "Location";
                            row.Col2 = "Direction";
                            row.Col3 = "Count2";
                            row.Col5 = "Currency";
                            row.Col6 = "Principal";
                            row.Col7 = "Send Charges";
                            row.Col8 = "Charges";
                            row.Col9 = "FX";
                            row.Col10 = "Total";
                            row.Col11 = "Total A";
                            count++;
                        }
                        else
                        {
                            var index0 = reader.GetValue(0)?.ToString();
                            var index1 = reader.GetValue(1)?.ToString();
                            try
                            {
                                if (index0 != null)
                                {
                                    if (index0.Contains("Location"))
                                    {
                                        location = index0.Split(':')[1];
                                        IOBound = "Total";
                                    }
                                    if (index0.Contains("Date Range"))
                                    {
                                        date = index0.Replace("Date Range", "");
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }

                            //logic for direction
                            if (index1 != null)
                            {
                                if (index1.Contains("Direction"))
                                {
                                    IOBound = index1;

                                }
                            }
                            row.Col0 = date;


                            row.Col1 = location;
                            row.Col2 = IOBound;

                            if (index0 != null)
                            {

                                if (index0.Contains("Count"))
                                {
                                    //count
                                    row.Col3 = reader.GetValue(0)?.ToString().Replace("\n", "");
                                    //count value
                                    row.Col4 = reader.GetValue(3)?.ToString().Replace("\n", "");

                                    row.Col5 = reader.GetValue(5)?.ToString().Replace("\n", "");

                                    row.Col6 = reader.GetValue(6)?.ToString().Replace("\n", "");

                                    row.Col7 = reader.GetValue(9)?.ToString().Replace("\n", "");

                                    row.Col8 = reader.GetValue(12)?.ToString().Replace("\n", "");

                                    row.Col9 = reader.GetValue(15)?.ToString().Replace("\n", "");

                                    row.Col10 = reader.GetValue(18)?.ToString().Replace("\n", "");

                                    row.Col11 = reader.GetValue(21)?.ToString().Replace("\n", "");

                                }
                                else if (index0.Contains("Location") || index0.Contains("KICUKIRO")
                                    || index0.Contains("Agent") || index0.Contains("RWF"))
                                {
                                    continue;
                                }
                                else
                                {
                                    row.Col3 = reader.GetValue(0)?.ToString().Replace("\n", "");

                                    row.Col4 = reader.GetValue(1)?.ToString().Replace("\n", "");

                                    row.Col5 = reader.GetValue(4)?.ToString().Replace("\n", "");

                                    row.Col6 = reader.GetValue(5)?.ToString().Replace("\n", "");

                                    row.Col7 = reader.GetValue(8)?.ToString().Replace("\n", "");

                                    row.Col8 = reader.GetValue(11)?.ToString().Replace("\n", "");

                                    row.Col9 = reader.GetValue(14)?.ToString().Replace("\n", "");

                                    row.Col10 = reader.GetValue(17)?.ToString().Replace("\n", "");

                                    row.Col11 = reader.GetValue(20)?.ToString().Replace("\n", "");
                                }

                            }
                            else
                            {
                                row.Col3 = reader.GetValue(0)?.ToString().Replace("\n", "");

                                row.Col4 = reader.GetValue(1)?.ToString().Replace("\n", "");

                                row.Col5 = reader.GetValue(4)?.ToString().Replace("\n", "");

                                row.Col6 = reader.GetValue(5)?.ToString().Replace("\n", "");

                                row.Col7 = reader.GetValue(8)?.ToString().Replace("\n", "");

                                row.Col8 = reader.GetValue(11)?.ToString().Replace("\n", "");

                                row.Col9 = reader.GetValue(14)?.ToString().Replace("\n", "");

                                row.Col10 = reader.GetValue(17)?.ToString().Replace("\n", "");

                                row.Col11 = reader.GetValue(20)?.ToString().Replace("\n", "");

                            }
                        }

                        if (row.Col4 != null && row.Col4.Contains("Direction"))
                        {
                            continue;
                        }

                        list.Add(row);
                    }
                }
            }

            var finalList = new List<ExcelCols>();
            finalList.Add(list[0]);
            list.Remove(list[0]);

            foreach (var rows in list)
            {
                rows.Col0 = list[list.Count - 1].Col0;
                finalList.Add(rows);
            }

            //finalList.Remove(list[list.Count-1]);
            finalList[finalList.Count - 1].Col2 = "Grand_Total";
            finalList[finalList.Count - 2].Col2 = "Grand_Total";
            finalList[finalList.Count - 3].Col2 = "Settlement";
            finalList[finalList.Count - 4].Col2 = "Settlement";

            var output = new List<ExcelCols>();

            foreach (var rows in finalList)
            {
                if (rows.Col10 == null && rows.Col11 == null)
                {
                    continue;
                }

                output.Add(rows);
            }


            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_ADV_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(output, outputFile);
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