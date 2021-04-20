using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.BNR
{
    public class SettlementConverter
    {
        public SettlementConverter()
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
                    string reference = "Reference";

                    string code = "Codes";

                    string value_date = "Value Date";

                    string type = "Type";

                    string debit_account = "Debit Account";

                    string ordering_customer = "Ordering Customer/Drawer";

                    string credit_account = "Credit Account";

                    string beneficiary = "Beneficiary";

                    string remmittance_infos = "Remittance infos";

                    string amount = "Amount";

                    string input_time = "Input Time";

                    string status = "Status";

                    string modification_time = "Modification Time";

                    string status2 = "Status2";

                    string DR_CR = "DR_CR";

                    string Type_id = "Type_id";

                    int countHeader = 0;

                    while (reader.Read())
                    {
                        var row = new ExcelCols();

                        var value = reader.GetValue(0)?.ToString();

                        //Ignore the colunm that contain DIFF or MT971
                        if (value.Contains("DIFF"))
                        {
                            continue;
                        }
                        else if (value.Contains("MT971"))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Outw TSF") && reader.GetValue(6).ToString().Contains("USD"))
                        {
                            row.Col4 = "3208000-USD";
                            row.Col14 = "Credit";

                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Inw TSF") && reader.GetValue(6).ToString().Contains("USD"))
                        {
                            row.Col6 = "3208000-USD";
                            row.Col14 = "Debit";

                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Outw CHQ") && reader.GetValue(6).ToString().Contains("USD"))
                        {
                            row.Col4 = "3208000-USD";
                            row.Col14 = "Debit";

                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Inw CHQ") && reader.GetValue(6).ToString().Contains("USD"))
                        {
                            row.Col6 = "3208000-USD";
                            row.Col14 = "Credit";

                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Outw TSF") && reader.GetValue(6).ToString().Contains("RWF"))
                        {
                            row.Col4 = "1240000-RWF";
                            row.Col14 = "Credit";

                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Inw TSF") && reader.GetValue(6).ToString().Contains("RWF"))
                        {
                            row.Col6 = "1240000-RWF";
                            row.Col14 = "Debit";

                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Outw CHQ") && reader.GetValue(6).ToString().Contains("RWF"))
                        {
                            row.Col4 = "1240000-RWF";
                            row.Col14 = "Debit";
                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Inw CHQ") && reader.GetValue(6).ToString().Contains("RWF"))
                        {
                            row.Col6 = "1240000-RWF";
                            row.Col14 = "Credit";
                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Outw TSF") && reader.GetValue(6).ToString().Contains("EUR"))
                        {
                            row.Col4 = "1000026561-EUR";
                            row.Col14 = "Credit";
                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Inw TSF") && reader.GetValue(6).ToString().Contains("EUR"))
                        {
                            row.Col6 = "1000026561-EUR";
                            row.Col14 = "Debit";
                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Outw CHQ") && reader.GetValue(6).ToString().Contains("EUR"))
                        {
                            row.Col4 = "1000026561-EUR";
                            row.Col14 = "Debit";
                        }
                        else if (reader.GetValue(5) != null && reader.GetValue(6) != null && reader.GetValue(5).ToString().Contains("Settl. Inw CHQ") && reader.GetValue(6).ToString().Contains("EUR"))
                        {
                            row.Col6 = "1000026561-EUR";
                            row.Col14 = "Credit";

                        }

                        //Format into date
                        var date = reader.GetValue(5)?.ToString();

                        row.Col2 = date.Substring(Math.Max(0, date.Length - 10), Math.Min(10, date.Length)).Replace(".", "-");

                        row.Col7 = reader.GetValue(5)?.ToString().Replace("\n", "");

                        row.Col9 = reader.GetValue(7)?.ToString().Replace("\n", "");

                        if (countHeader == 0)
                        {
                            row.Col0 = reference;
                            row.Col1 = code;
                            row.Col2 = value_date;
                            row.Col3 = type;
                            row.Col4 = debit_account;
                            row.Col5 = ordering_customer;
                            row.Col6 = credit_account;
                            row.Col7 = beneficiary;
                            row.Col8 = remmittance_infos;
                            row.Col9 = amount;
                            row.Col10 = input_time;
                            row.Col11 = status;
                            row.Col12 = modification_time;
                            row.Col13 = status2;
                            row.Col14 = DR_CR;
                            row.Col15 = Type_id;

                            countHeader++;
                        }
                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.GetDirectoryName(inputFile);

                outputFolder = Path.Combine(Directory.GetParent(outputFolder).FullName, "Conv");

                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder, $"{DateTime.Now:yyyy_MM_dd_HH_mm}_{fileName.Substring(Math.Max(0, fileName.Length - 10))}_SETMT.csv");
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
