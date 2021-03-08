using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters
{
    public class BnrConverter
    {
        public BnrConverter()
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
                    string code = "Codes";

                    string status = "Status 2";

                    string DR_CR = "DR_CR";

                    string Type_id = "Type_id";

                    int countHeader = 0;

                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        var val = reader.GetValue(2)?.ToString();

                        if (val != null && reader.GetValue(2).ToString().StartsWith("Code"))
                        {
                            code = reader.GetValue(2)?.ToString();
                        }

                        var value = reader.GetValue(4)?.ToString();

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        if (code.Equals("Code - 032"))
                        {
                            row.Col15 = "MT104";
                        }
                        else if (code.Equals("Code - 012"))
                        {
                            row.Col15 = "MT971";
                        }
                        else if (code.Equals("Code - 011"))
                        {
                            row.Col15 = "MT971";
                        }
                        else if (reader.GetValue(5) != null &&
                          !code.Equals("Code - 011") &&
                          !code.Equals("Code - 012") &&
                          reader.GetValue(5).ToString().Equals("pacs.009. 001.08"))
                        {
                            row.Col15 = "MT202";
                        }
                        else if (reader.GetValue(19) != null &&
                          !code.Equals("Code - 032") &&
                          reader.GetValue(19)?.ToString() == "Active" ||
                          reader.GetValue(19)?.ToString() == "Rejected")
                        {
                            row.Col15 = "MT102";

                        }
                        else if (row.Col3 != null && row.Col13 != null &&
                           row.Col3.Contains("pacs.008. 001.08") &&
                           row.Col13.Contains("Bulk"))
                        {
                            row.Col15 = "MT102";
                        }
                        else
                        {
                            row.Col15 = "MT103";
                        }
                        //the value at index 0 is null for the child row hence the check
                        if (reader.GetValue(19)?.ToString() == "Active" || reader.GetValue(19)?.ToString() == "Rejected")
                        {
                            //logic to read child columns

                            //Reference
                            row.Col0 = reader.GetValue(4)?.ToString().Replace("\n", "");

                            //Codes colunm
                            row.Col1 = list.Last().Col1;
                            //Value Date
                            row.Col2 = list.Last().Col2;

                            row.Col3 = list.Last().Col3;
                            //Debit account
                            row.Col4 = list.Last().Col4;
                            //Odering customer
                            row.Col5 = reader.GetValue(7)?.ToString() + reader.GetValue(10)?.ToString();
                            //Credit account
                            row.Col6 = list.Last().Col6;

                            row.Col7 = reader.GetValue(13)?.ToString() + reader.GetValue(14)?.ToString();

                            row.Col8 = list.Last().Col8;
                            //Amount
                            row.Col9 = reader.GetValue(18)?.ToString(); ;
                            //Input time
                            row.Col10 = list.Last().Col10;
                            //Status
                            row.Col11 = list.Last().Col11;
                            //Modification time
                            row.Col12 = list.Last().Col12;

                            //(Active) Status of subdirectory
                            row.Col13 = reader.GetValue(19)?.ToString();

                            //method for the bulk colunm
                            if (string.IsNullOrEmpty(list.Last().Col13?.ToLower()))
                            {
                                list.Last().Col13 = "Bulk";
                                if (list.Last().Col3.Contains("pacs.008. 001.08"))
                                {
                                    list.Last().Col15 = "MT102";
                                }
                            }
                            if (row.Col4 != null && row.Col4.StartsWith("1240000") || row.Col4.StartsWith("3208000") || row.Col4.StartsWith("1000026561"))
                            {
                                row.Col14 = "Debit";
                            }
                            else
                            {
                                row.Col14 = "Credit";
                            }
                            list.Add(row);
                        }
                        else
                        {
                            //logic for parent
                            if (reader.GetValue(6) != null && reader.GetValue(6).ToString().Replace("\n", "").StartsWith("1240000") || reader.GetValue(6).ToString().Replace("\n", "").StartsWith("3208000") || reader.GetValue(6).ToString().Replace("\n", "").StartsWith("1000026561"))
                            {
                                row.Col14 = "Debit";
                            }
                            else
                            {
                                row.Col14 = "Credit";
                            }

                            //Reference
                            row.Col0 = reader.GetValue(0)?.ToString().Replace("\n", "");

                            //Codes colunm
                            row.Col1 = code;

                            //Value Date
                            row.Col2 = reader.GetValue(4)?.ToString();

                            //Type
                            row.Col3 = reader.GetValue(5)?.ToString();

                            //Debit Account
                            row.Col4 = reader.GetValue(6)?.ToString().Replace("\n", "");

                            //Odering Customer/Drawer
                            row.Col5 = reader.GetValue(8)?.ToString();

                            //Credit Account
                            row.Col6 = reader.GetValue(11)?.ToString().Replace("\n", "");

                            //Beneficiary
                            row.Col7 = reader.GetValue(12)?.ToString();

                            //Remittance infos
                            row.Col8 = reader.GetValue(13)?.ToString();

                            //Amount
                            row.Col9 = reader.GetValue(14)?.ToString();

                            //Input Time
                            row.Col10 = reader.GetValue(15)?.ToString();

                            //Status
                            row.Col11 = reader.GetValue(17)?.ToString();

                            //Modification Time
                            row.Col12 = reader.GetValue(18)?.ToString();

                            if (countHeader == 0)
                            {
                                row.Col13 = status;
                                row.Col14 = DR_CR;
                                row.Col15 = Type_id;

                                countHeader++;
                            }
                            list.Add(row);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.csv");
            }

            WriteToFile(list, outputFile);

            GenerateMultiCurr(list);
        }

        private void GenerateMultiCurr(List<ExcelCols> list)
        {
            throw new NotImplementedException("Error generating Multicurr file for BNR");
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
    }
}
