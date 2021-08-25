using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

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
            var list = new List<ExcelCols>();

            var lines = File.ReadAllLines(inputFile);

            var previousIs82A_1 = false;

            var previousIs87A_1 = false;

            var previousIs53A_1 = false;

            var previousIs53A_2 = false;

            var previousIs57A_1 = false;

            var previousIs57A_2 = false;

            var first57AProcessed = false;

            var first53AProcessed = false;

            var countHeader = 0;

            var senderA = "Party A - BIC";

            var recieverB = "Party B - BIC";

            var newSequence15A = "New Sequence A";

            var newSequence15B = "New Sequence B";

            var newSequence15C = "New Sequence C";

            var newSequence15D = "New Sequence D";

            var senderRef = "Sender Reference";

            var typeOfEvent = "Type Of Event";

            var typeOperation = "Type of operation";

            var commonRef = "Common Reference";

            var partysARole = "Partys A role";

            var tradeDate = "Trade date";

            var valueDate = "Value date";

            var contractNoPartyA = "Contract NO";

            var dayCountFraction = "Day Count";

            var currency32 = "Currency";

            var principalAmount = "Principal Amount";

            var currency34E = "Currency";

            var interestAmt = "Interest Amount";

            var deliveryAgent1 = "Delivery Agent 1 ";

            var recAgent1 = "Receiving Agent 1";

            var deliveryAgent2 = "Delivery Agent 2 ";

            var recAgent2 = "Receiving Agent 2";

            var interestRateDueDate = "Interest Date";

            var interestRate = "Interest Rate";

            var scopeOfOperation = "Scope Of Operation";

            var maturityDate = "Maturity Date";

            var row = new ExcelCols();

            foreach (var line in lines)
            {
                if (countHeader == 0)
                {
                    var header = new ExcelCols();

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
                    var value = line.Replace("\n", "");

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
                        previousIs82A_1 = true;
                        row.Col7 = value.Split(':')[2].Replace("\n", "");
                        continue;
                    }
                    //check change
                    else if (value.StartsWith(":87A:"))
                    {
                        previousIs87A_1 = true;
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
                        row.Col26 = row.Col26 + " " +  value.Split(':')[2].Replace("\n", "");
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
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MT320_{entity}_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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