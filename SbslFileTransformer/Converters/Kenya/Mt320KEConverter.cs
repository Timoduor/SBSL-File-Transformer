using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Kenya
{
    public class Mt320KEConverter
    {
        public Mt320KEConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null, string entity = "IMKE")
        {
            List<ExcelCols> list = new List<ExcelCols>();

            string[] lines = File.ReadAllLines(inputFile);

            bool previousIs53A_1 = false;

            bool previousIs53A_2 = false;

            bool previousIs57A_1 = false;

            bool previousIs57A_2 = false;

            bool first57AProcessed = false;

            bool first53AProcessed = false;

            int countHeader = 0;

            string senderA = "Party A - BIC";

            string recieverB = "Party B - BIC";

            string newSequence15A = "New Sequence A";

            string newSequence15B = "New Sequence B";

            string newSequence15C = "New Sequence C";

            string newSequence15D = "New Sequence D";

            string senderRef = "Sender Reference";

            string typeOfEvent = "Type Of Event";

            string typeOperation = "Type of operation";

            string commonRef = "Common Reference";

            string partysARole = "Partys A role";

            string tradeDate = "Trade date";

            string valueDate = "Value date";

            string contractNoPartyA = "Contract NO";

            string dayCountFraction = "Day Count";

            string currency32 = "Currency";

            string principalAmount = "Principal Amount";

            string currency34E = "Currency";

            string interestAmt = "Interest Amount";

            string deliveryAgent1 = "Delivery Agent 1 ";

            string recAgent1 = "Receiving Agent 1";

            string deliveryAgent2 = "Delivery Agent 2 ";

            string recAgent2 = "Receiving Agent 2";

            string interestRateDueDate = "Interest Date";

            string interestRate = "Interest Rate";

            string scopeOfOperation = "Scope Of Operation";

            string maturityDate = "Maturity Date";

            ExcelCols row = new ExcelCols();

            foreach (string line in lines)
            {
                if (countHeader == 0)
                {
                    ExcelCols header = new ExcelCols();

                    header.Col0 = newSequence15A;

                    header.Col1 = senderRef;

                    header.Col2 = typeOperation;

                    header.Col3 = scopeOfOperation;

                    header.Col4 = typeOfEvent;

                    header.Col5 = commonRef;

                    header.Col6 = contractNoPartyA;

                    header.Col7 = senderA;

                    header.Col8 = recieverB;

                    header.Col9 = newSequence15B;

                    header.Col10 = partysARole;

                    header.Col11 = tradeDate;

                    header.Col12 = valueDate;

                    header.Col13 = maturityDate;

                    header.Col14 = currency32;

                    header.Col15 = principalAmount;

                    header.Col16 = interestRateDueDate;

                    header.Col17 = currency34E;

                    header.Col18 = interestAmt;

                    header.Col19 = interestRate;

                    header.Col20 = dayCountFraction;

                    header.Col21 = newSequence15C;

                    header.Col22 = deliveryAgent1;

                    header.Col23 = recAgent1;

                    header.Col24 = newSequence15D;

                    header.Col25 = deliveryAgent2;

                    header.Col26 = recAgent2;

                    list.Add(header);
                }
                else
                {
                    row.Col26 = Path.GetFileNameWithoutExtension(inputFile);
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
                    else if (value.StartsWith(":22A:"))
                    {
                        row.Col2 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":94A:"))
                    {
                        row.Col3 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":22B:"))
                    {
                        row.Col4 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":22C:"))
                    {
                        row.Col5 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":21N:"))
                    {
                        row.Col6 = value.Split(':')[2].Replace("\n", "");
                    }
                    //check change
                    else if (value.StartsWith(":82A:"))
                    {
                        row.Col7 = value.Split(':')[2].Replace("\n", "");
                        continue;
                    }
                    //check change
                    else if (value.StartsWith(":87A:"))
                    {
                        row.Col8 = value.Split(':')[2].Replace("\n", "");
                        continue;
                    }
                    else if (value.StartsWith(":15B:"))
                    {
                        row.Col9 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":17R:"))
                    {
                        row.Col10 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":30T:"))
                    {
                        row.Col11 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":30V:"))
                    {
                        row.Col12 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":30P:"))
                    {
                        row.Col13 = value.Split(':')[2].Replace("\n", "");
                    }
                    //check change
                    else if (value.StartsWith(":32B:"))
                    {
                        //currency
                        row.Col14 = value.Substring(5, 3);
                        //principal Amount
                        row.Col15 = value.Substring(8).Replace(",", ".");
                    }
                    else if (value.StartsWith(":30X:"))
                    {
                        row.Col16 = value.Split(':')[2].Replace(",", "").TrimEnd();
                    }
                    //check change
                    else if (value.StartsWith(":34E:"))
                    {
                        //currency
                        row.Col17 = value.Substring(5, 3);
                        //principle amount
                        row.Col18 = value.Substring(8).Replace(",", ".");
                    }
                    //check change
                    else if (value.StartsWith(":37G:"))
                    {
                        row.Col19 = value.Split(':')[2].Replace(",", ".").TrimEnd();
                    }
                    else if (value.StartsWith(":14D:"))
                    {
                        row.Col20 = value.Split(':')[2].Replace(",", "").TrimEnd();
                    }
                    else if (value.StartsWith(":15C:"))
                    {
                        row.Col21 = value.Split(':')[2].Replace("\n", "");
                    }
                    else if (value.StartsWith(":53A:") && !first53AProcessed)
                    {
                        previousIs53A_1 = true;
                        row.Col22 = value.Split(':')[2].Replace(",", "").TrimEnd();
                        first53AProcessed = true;
                        continue;
                    }

                    else if (value.StartsWith(":57A:") && !first57AProcessed)
                    {
                        previousIs57A_1 = true;
                        row.Col23 = value.Split(':')[2].Replace("\n", "");
                        first57AProcessed = true;
                        continue;
                    }
                    else if (value.StartsWith(":15D:"))
                    {
                        row.Col24 = value.Split(':')[2].Replace(",", "").TrimEnd();
                    }
                    else if (value.StartsWith(":53A:") && first53AProcessed)
                    {
                        previousIs53A_2 = true;
                        row.Col25 = value.Split(':')[2].Replace(",", "").TrimEnd();
                        continue;
                    }
                    else if (value.StartsWith(":57A:") && first57AProcessed)
                    {
                        previousIs57A_2 = true;
                        row.Col26 = row.Col26 + " " + value.Split(':')[2].Replace("\n", "");
                        continue;
                    }

                    //check change
                    //if (previousIs82A_1 && !value.StartsWith(":") && !value.StartsWith("-}") && !value.StartsWith("/"))
                    //{
                    //    row.Col7 = value;
                    //    previousIs82A_1 = false;
                    //}

                    //if (previousIs87A_1 && !value.StartsWith(":") && !value.StartsWith("-}") && !value.StartsWith("/"))
                    //{
                    //    row.Col8 = value;
                    //    previousIs87A_1 = false;
                    //}

                    if (previousIs53A_1 && !value.StartsWith(":") && !value.StartsWith("-}") && !value.StartsWith("/"))
                    {
                        row.Col22 = value;
                        previousIs53A_1 = false;
                    }

                    if (previousIs53A_2 && !value.StartsWith(":") && !value.StartsWith("-}") && !value.StartsWith("/"))
                    {
                        row.Col25 = value;
                        previousIs53A_2 = false;
                    }

                    if (previousIs57A_1 && !value.StartsWith(":") && !value.StartsWith("-}") && !value.StartsWith("/"))
                    {
                        row.Col23 = value;
                        previousIs57A_1 = false;
                    }

                    if (previousIs57A_2 && !value.StartsWith(":") && !value.StartsWith("-}") && !value.StartsWith("/"))
                    {
                        row.Col26 = value;
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
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MT320_{entity}_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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