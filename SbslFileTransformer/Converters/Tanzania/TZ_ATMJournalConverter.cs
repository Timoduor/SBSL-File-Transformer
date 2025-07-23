using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using CsvHelper;

namespace SbslFileTransformer.Converters.Tanzania
{
    public class TZ_ATMJournalConverter
    {
        public void ProcessATMjournalFile(string inputFile, string outputFile = "", string entity = "IMTZ")
        {
            var atmJrnlTransactions = new List<string>();

            var builder = string.Empty;
            var transactionStarted = false;

            // Read the input file line by line and build transactions
            foreach (var line in File.ReadLines(inputFile))
            {
                if (line.Contains("Transaction Start", StringComparison.OrdinalIgnoreCase))
                {
                    _ = builder += line;
                    transactionStarted = true;
                }
                else if (transactionStarted && line.Contains("Transaction End", StringComparison.OrdinalIgnoreCase))
                {
                    builder += $" & {line}";
                    atmJrnlTransactions.Add(builder);
                    builder = string.Empty;
                    transactionStarted = false;
                }
                else if (transactionStarted)
                {
                    _ = builder += $" & {line}";
                }
            }

            //pick only transactions that contain "Cash Taken"
            for (var i = 0; i < atmJrnlTransactions.Count; i++)
            {
                if (!atmJrnlTransactions[i].Contains("Cash Taken", StringComparison.OrdinalIgnoreCase))
                {
                    atmJrnlTransactions.RemoveAt(i);
                    i--;
                }
            }

            var atmJournals = new List<TzAtmJournal>();

            var regexPairs = new Dictionary<string, string>
            {
                ["refNo"] = @"REF\.?NO:\s*(?<value>\d+)",
                ["cardNo"] = @"CRD:\s*(?<value>\d{6}X{6}\d{4})",
                ["cardNo2"] = @"EMV AID.*?\/\s+(?<value>\d{6}\*{6}\d{4})",
                ["cardNo3"] = @"EMV AID.*?\/\s+(?<value>\d{6}X{6}\d{4})",
                ["amount"] = @"DISP:\s*[A-Z]{3}\s*(?<value>[\d,]+\.\d{2})",
                ["currency"] = @"DISP:\s*(?<value>[A-Z]{3})",
                ["transDate"] = @"(?<value>\d{2}/\d{2}/\d{2})\s+\d{2}:\d{2}:\d{2}",
                ["transDate2"] = @"(?<value>\d{2}.\d{2}.\d{2})\s+\d{2}:\d{2}",
                ["transTime"] = @"\d{2}/\d{2}/\d{2}\s+(?<value>\d{2}:\d{2}:\d{2})",
                ["transTime2"] = @"\d{2}.\d{2}.\d{2}\s+(?<value>\d{2}:\d{2})",
                ["reasonCode"] = @"RESP:\s*(?<value>\d+)",
                ["successful"] = @"(?<value>REQ SERVICED|DECLINED|FAILED)",
                ["atmNo"] = @"(?<value>\d{8})\s+\d{2}\.\d{2}\.\d{2}",
                ["atmNo2"] = @"(\d{2}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}).*(?<value>[^\s]{8})",
                ["refAmtCurr"] = @"(?<reference>\d+/\d+)\s+(?<amount>[+-]?\d+\.\d{2})\s+(?<currency>[A-Z]{3})",
            };

            foreach (var line in atmJrnlTransactions)
            {
                var matches = new Dictionary<string, string>();

                foreach (var pair in regexPairs)
                {
                    var match = Regex.Match(line, pair.Value);

                    if (match.Success)
                    {
                        if (!string.IsNullOrEmpty(match.Groups["value"].Value.Trim()))
                        {
                            matches[pair.Key] = match.Groups["value"].Value.Trim();
                        }
                        else if (pair.Key == "refAmtCurr")
                        {
                            if (!string.IsNullOrEmpty(match.Groups["reference"].Value) &&
                               !string.IsNullOrEmpty(match.Groups["amount"].Value) &&
                               !string.IsNullOrEmpty(match.Groups["currency"].Value))
                            {
                                matches["refNo"] = match.Groups["reference"].Value.Trim();
                                matches["amount"] = match.Groups["amount"].Value.Trim();
                                matches["currency"] = match.Groups["currency"].Value.Trim();
                            }
                        }
                    }
                }

                var journal = new TzAtmJournal();

                journal.Amount = matches.TryGetValue("amount", out var value) ? value.Trim() : "";
                journal.Reference = matches.TryGetValue("refNo", out value) ? value.Trim() : "";
                journal.CardNo = matches.TryGetValue("cardNo", out value) ? value.Trim() :
                    matches.TryGetValue("cardNo2", out value) ? value.Trim() :
                    matches.TryGetValue("cardNo3", out value) ? value.Trim() : "";
                journal.Successful = matches.TryGetValue("successful", out value) ? value.Trim() : "";
                journal.ReasonCode = matches.TryGetValue("reasonCode", out value) ? value.Trim() : "";
                journal.Currency = matches.TryGetValue("currency", out value) ? value.Trim() : "";
                journal.AtmNo = matches.TryGetValue("atmNo", out value) ? value.Trim() :
                    matches.TryGetValue("atmNo2", out value) ? value.Trim() : "";


                if (matches.TryGetValue("transDate", out var transDateValue) && matches.TryGetValue("transTime", out var transTimeValue) && DateTime.TryParseExact(transDateValue.Trim() + " " + transTimeValue.Trim(), "yy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var transDate))
                {
                    journal.TrnDate = transDate;
                }
                else if (matches.TryGetValue("transDate2", out var transDateValue2) && matches.TryGetValue("transTime2", out var transTimeValue2) && DateTime.TryParseExact(transDateValue2.Trim() + " " + transTimeValue2.Trim(), "yy.MM.dd HH:mm", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var transDate2))
                {
                    journal.TrnDate = transDate2;
                }
                else
                {
                    throw new Exception("Unable to get proper date format from journal entry!");
                }

                atmJournals.Add(journal);
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                _ = Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_AtmJnrl_{entity}_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            WriteToFile(atmJournals, outputFile);
        }

        private void WriteToFile(List<TzAtmJournal> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<TzAtmJournal>();
                    csv.NextRecord();

                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }

        private class TzAtmJournal
        {
            public string CardNo { get; set; }

            public DateTime TrnDate { get; set; }

            public string Amount { get; set; }

            public string UtrnNo { get; set; }

            public string Successful { get; set; }

            public string ReasonCode { get; set; }

            public string AtmNo { get; set; }

            public string AuthNo { get; set; }

            public string AmountRemaining { get; set; }

            public string Currency { get; set; }

            public string Reference { get; set; }
        }
    }
}
