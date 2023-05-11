using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Kenya
{
    //PesaLink_GL2
    public class PesaLinkStatementConverter
    {
        public PesaLinkStatementConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
                {

                    int countHeader = 0;

                    string endtoend = "End-to-end ID";

                    string direction = "Direction";

                    string initiationChannell = "Initiation Channel";

                    string settlementDate = "Settlement Date";

                    string transactionAmount = "Transaction Amount";

                    string iPSR = "IPS Reception date and time";

                    string iPSC = "IPS Completion date and time";

                    string transactionType = "Transaction Type";

                    string processingType = "Processing Type";

                    string finalStatus = "Final Status";

                    string reasonCode = "Reason Code";

                    string originatorPIC = "Originator PIC";

                    string receiverPIC = "Receiver PIC";

                    string debtorName = "Debtor Name";

                    string debtorAccount = "Debtor Account";

                    string creditorName = "Creditor Name";

                    string creditorAccount = "Creditor Account";

                    string narration = "Narration";


                    while (reader.Read())
                    {
                        var row = new ExcelCols();


                        row.Col0 = reader.GetValue(0)?.ToString();

                        row.Col1 = reader.GetValue(1)?.ToString();

                        row.Col2 = reader.GetValue(2)?.ToString();

                        //row.Col3 = reader.GetValue(3)?.ToString();

                        //row.Col4 = reader.GetValue(4)?.ToString();

                        if (string.IsNullOrEmpty(row.Col0) && string.IsNullOrEmpty(row.Col1) &&
                           string.IsNullOrEmpty(row.Col2) && string.IsNullOrEmpty(row.Col3))
                        {
                            continue;
                        }

                        if (row.Col0.Contains("PARTICIPANT NAME"))
                        {
                            countHeader++;
                            list.Add(row);
                            continue;
                        }

                        if (countHeader == 4)
                        {
                            list[3].Col1 = row.Col0;
                            list[3].Col2 = "";
                        }

                        if (countHeader == 4)
                        {

                            row.Col0 = endtoend;

                            row.Col1 = direction;

                            row.Col2 = initiationChannell;

                            row.Col3 = settlementDate;

                            row.Col4 = transactionAmount;

                            row.Col5 = iPSR;

                            row.Col6 = iPSC;

                            row.Col7 = transactionType;

                            row.Col8 = processingType;

                            row.Col9 = finalStatus;

                            row.Col10 = reasonCode;

                            row.Col11 = originatorPIC;

                            row.Col12 = receiverPIC;

                            row.Col13 = debtorName;

                            row.Col14 = debtorAccount;

                            row.Col15 = creditorName;

                            row.Col16 = creditorAccount;

                            row.Col17 = narration;

                        }


                        if (countHeader == 5)
                        {
                            row.Col1 = "C";
                            row.Col2 = "";
                            row.Col3 = list[2].Col1.Substring(0);
                            row.Col4 = reader.GetValue(4)?.ToString();
                            row.Col5 = list[2].Col1.Substring(0);
                            row.Col6 = list[2].Col1.Substring(0);
                            row.Col7 = "pacs.008";
                            row.Col8 = "SinglePayment";
                            row.Col9 = "ACCP";
                            row.Col11 = "0003";
                            row.Col12 = "0057";
                            row.Col13 = list[1].Col1.Substring(0);
                            row.Col17 = "KESIPSL";


                            list.Add(row);

                            break;
                        }
                        countHeader++;

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
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_PesaLink_{fileName.Substring(0, Math.Min(fileName.Length, 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(list, outputFile);
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
