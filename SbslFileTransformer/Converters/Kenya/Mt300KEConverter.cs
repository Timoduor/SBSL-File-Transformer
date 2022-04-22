using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Kenya
{
    public class Mt300KEConverter
    {
        public Mt300KEConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null, string entity = "IMKE")
        {
            List<ExcelCols> list = new List<ExcelCols>();

            string[] lines = File.ReadAllLines(inputFile);

            bool previousIs57A_1 = false;

            bool previousIs57A_2 = false;

            bool first57AProcessed = false;

            int countHeader = 0;

            string senderA = "Party A - BIC";

            string recieverB = "Party B - BIC";

            string newSequence15A = "New Sequence A";

            string newSequence15B = "New Sequence B";

            string newSequence15C = "New Sequence C";

            string senderRef = "Sender Reference";

            string relatedRef = "Related Reference";

            string typeOperation = "Type of operation";

            string commonRef = "Common Reference";

            string tradeDate = "Trade date";

            string valueDate = "Value date";

            string exchangeRate = "Exchange Rate";

            string Amount32 = "Amount";

            string currency32 = "Currency";

            string currency33 = "Currency";

            string Amount33 = "Amount";

            string recAgent = "Receiving Agent - FI BIC";

            string scopeOfOperation = "Scope Of Operation";

            string nonDelivarableOperator = "Non-Deliverable Indicator";

            string brokerIdentification = "Broker Identification -Name&Addr";


            ExcelCols row = new ExcelCols();

            foreach (string line in lines)
            {
                if (countHeader == 0)
                {
                    ExcelCols header = new ExcelCols();

                    header.Col0 = newSequence15A;

                    header.Col1 = senderRef;

                    header.Col2 = relatedRef;

                    header.Col3 = typeOperation;

                    header.Col4 = scopeOfOperation;

                    header.Col5 = commonRef;

                    header.Col6 = senderA;

                    header.Col7 = recieverB;

                    header.Col8 = nonDelivarableOperator;

                    header.Col9 = newSequence15B;

                    header.Col10 = tradeDate;

                    header.Col11 = valueDate;

                    header.Col12 = exchangeRate;

                    header.Col13 = currency32;

                    header.Col14 = Amount32;

                    header.Col15 = recAgent;

                    header.Col16 = currency33;

                    header.Col17 = Amount33;

                    header.Col18 = recAgent;

                    header.Col19 = newSequence15C;

                    header.Col20 = brokerIdentification;

                    list.Add(header);
                }
                else
                {
                    row.Col20 = Path.GetFileNameWithoutExtension(inputFile);
                    string value = line.Replace("\n", "");

                    if (value.StartsWith(":15A:"))
                    {
                        //new sequence
                        row.Col0 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":20:"))
                    {
                        //sender reference
                        row.Col1 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":21:"))
                    {
                        row.Col2 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":22A:"))
                    {
                        row.Col3 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":94A:"))
                    {
                        row.Col4 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":22C:"))
                    {
                        row.Col5 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":82A:"))
                    {
                        row.Col6 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":87A:"))
                    {
                        row.Col7 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":17F:"))
                    {
                        row.Col8 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":15B:"))
                    {
                        row.Col9 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":30T:"))
                    {
                        row.Col10 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":30V:"))
                    {
                        row.Col11 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":36:"))
                    {
                        row.Col12 = value.Split(':')[2].Replace(",", ".");
                    }


                    else if (value.StartsWith(":32B:"))
                    {
                        //currency
                        row.Col13 = value.Substring(5, 3);
                        //amount
                        row.Col14 = value.Substring(8).Replace(",", ".");
                        if (row.Col14.Split(".")[1] == "") row.Col14.ToString();
                    }

                    //check change
                    else if (value.StartsWith(":57A:") && !first57AProcessed)
                    {
                        previousIs57A_1 = true;
                        row.Col15 = value.Split(':')[2].Replace("\n", "");
                        first57AProcessed = true;
                        continue;
                    }

                    //check change
                    else if (value.StartsWith(":33B:") || value.Contains(",00") || value.Contains(","))
                    {
                        //currency
                        row.Col16 = value.Substring(5, 3);
                        //amount
                        row.Col17 = value[8..].Replace(",", ".");
                        //if (row.Col17.Split(".")[1] == "")
                        //{
                        //    row.Col17.ToString();
                        //}
                    }
                    //check change
                    else if (value.StartsWith(":57A:") && first57AProcessed)
                    {
                        previousIs57A_2 = true;
                        row.Col18 = value.Split(':')[2].Replace("\n", "");
                        continue;
                    }
                    else if (value.StartsWith(":15C:"))
                    {
                        row.Col19 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":88D:"))
                    {
                        row.Col20 = row.Col20 + " " + value.Split(':')[2].Replace("\n", "");
                    }


                    if (previousIs57A_1 && !value.StartsWith(":") && !value.StartsWith("-}"))
                    {
                        row.Col15 = value;
                        previousIs57A_1 = false;
                    }

                    if (previousIs57A_2 && !value.StartsWith(":") && !value.StartsWith("-}"))
                    {
                        row.Col18 = value;
                        previousIs57A_2 = false;
                    }
                }

                countHeader++;
            }

            if (row != null) list.Add(row);

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MT300_{entity}_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(list, outputFile);
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