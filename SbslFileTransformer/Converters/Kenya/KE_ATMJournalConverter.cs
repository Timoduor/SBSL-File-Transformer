using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static iText.IO.Util.IntHashtable;

namespace SbslFileTransformer.Converters.Kenya
{
    public class KE_ATMJournalConverter
    {
        private static readonly Regex TxnStartRegex = new Regex(@"(?<StartTime>\d{2}:\d{2}:\d{2})\s+->\s*TRANSACTION START", RegexOptions.Compiled);
        private static readonly Regex AidCardRegex = new Regex(@"EMV AID (?<UTRN>A[0-9]{12,}) / (?<Card>\d{6}[*X]{6,}\d{4}) STARTED", RegexOptions.Compiled);
        private static readonly Regex AmountRegex = new Regex(@"AMOUNT (?<EnteredAmt>\d+) ENTERED", RegexOptions.Compiled);
        private static readonly Regex ReplyRegex = new Regex(@"TRANSACTION REPLY NEXT (?<RC>\d+) FUNCTION (?<AuthNo>[A-Z0-9]+)", RegexOptions.Compiled);
        private static readonly Regex ReferenceRegex = new Regex(@"(?<Reference>[A-Z0-9]+/\d+)\s+\+(?<Amount>\d+(\.\d{2})?)\s+(?<Currency>[A-Z]{3})", RegexOptions.Compiled);
        private static readonly Regex FallbackReferenceRegex = new Regex(@"([A-Z0-9]+/\d+)", RegexOptions.Compiled);
        private static readonly Regex FallbackCurrencyRegex = new Regex(@"\b(KES|USD|EUR|GBP)\b", RegexOptions.Compiled);
        private static readonly Regex DateLineRegex = new Regex(@"^\d+\s+(?<Date>\d{2}\.\d{2}\.\d{2})\s+(?<Hour>\d{2}:\d{2})", RegexOptions.Compiled);
        private static readonly Regex EndRegex = new Regex(@"(?<EndTime>\d{2}:\d{2}:\d{2})\s+<- TRANSACTION END", RegexOptions.Compiled);
        private static readonly Regex CashTakenRegex = new Regex(@"(?<CashTime>\d{2}:\d{2}:\d{2})\s+CASH TAKEN", RegexOptions.Compiled);

        // Compatibility Wrappers
        public void ConvertFile_WinkaATMjrn(string inputFile) => ProcessFile(inputFile, entity: "IMKE");
        public void ConvertFile_NCR(string inputFile) => ProcessFile(inputFile, entity: "IMKE");

        // Main processing method
        public void ProcessFile(string inputFile, string entity = "IMKE")
        {
            var text = File.ReadAllText(inputFile);
            var lines = text.Split('\n');
            var csv = new StringBuilder();
            csv.AppendLine("CARD NO,DATE,AMOUNT,CURRENCY,UTRN NO,TRAN STAT,RC,AUTH NO,ATM NO,TRANSACTION_START,TRANSACTION_END,REFERENCE,CASH TAKEN");

            for (int i = 0; i < lines.Length; i++)
            {
                var txnStartMatch = TxnStartRegex.Match(lines[i]);
                if (!txnStartMatch.Success) continue;

                string startTime = txnStartMatch.Groups["StartTime"].Value;
                string cardNo = "", utrnNo = "", aid = "", enteredAmt = "", rc = "", authNo = "", reference = "", amount = "", currency = "", atmNo = "", date = "", transactionEnd = "", tranStat = "", cashTakenTime = "";

                int blockEnd = Math.Min(i + 40, lines.Length);
                bool hasCard = false;
                string fallbackReference = "";
                string fallbackCurrency = "";

                for (int j = i; j < blockEnd; j++)
                {
                    if (AidCardRegex.IsMatch(lines[j]))
                    {
                        var m = AidCardRegex.Match(lines[j]);
                        aid = m.Groups["UTRN"].Value;
                        utrnNo = aid;
                        cardNo = m.Groups["Card"].Value;
                        hasCard = true;
                    }
                    if (AmountRegex.IsMatch(lines[j]))
                    {
                        enteredAmt = AmountRegex.Match(lines[j]).Groups["EnteredAmt"].Value;
                    }
                    if (ReplyRegex.IsMatch(lines[j]))
                    {
                        var m = ReplyRegex.Match(lines[j]);
                        rc = m.Groups["RC"].Value;
                        authNo = m.Groups["AuthNo"].Value;
                    }
                    // Extract Reference, amount and currency
                    if (ReferenceRegex.IsMatch(lines[j]))
                    {
                        var m = ReferenceRegex.Match(lines[j]);
                        reference = m.Groups["Reference"].Value;
                        amount = m.Groups["Amount"].Value;
                        currency = m.Groups["Currency"].Value;
                    }
                    else
                    {
                        // Fallback reference
                        var fallbackMatch = FallbackReferenceRegex.Match(lines[j]);
                        if (string.IsNullOrEmpty(reference) && fallbackMatch.Success)
                        {
                            fallbackReference = fallbackMatch.Groups[1].Value;
                        }
                        // Fallback currency
                        var currencyMatch = FallbackCurrencyRegex.Match(lines[j]);
                        if (string.IsNullOrEmpty(currency) && currencyMatch.Success)
                        {
                            fallbackCurrency = currencyMatch.Groups[1].Value;
                        }
                    }
                    // Extract date and ATM number
                    if (DateLineRegex.IsMatch(lines[j]))
                    {
                        var m = DateLineRegex.Match(lines[j]);
                        date = $"{m.Groups["Date"].Value} {m.Groups["Hour"].Value}";
                        atmNo = lines[j].Split(' ')[0];
                    }
                    // Exttract Transaction Endtime
                    if (EndRegex.IsMatch(lines[j]))
                    {
                        transactionEnd = EndRegex.Match(lines[j]).Groups["EndTime"].Value;
                        break;
                    }
                    // Determine Transaction status
                    if (lines[j].Contains("CASH PRESENTED"))
                    {
                        tranStat = "APPROVED";
                    }

                    if (lines[j].ToUpper().Contains("DECLINED") || lines[j].ToUpper().Contains("NOT SUFFICIENT FUNDS") || lines[j].ToUpper().Contains("EXCEEDS WITHDRAWAL AMOUNT LIMIT"))
                    {
                        tranStat = "DECLINED";
                    }
                    var cashTakenMatch = CashTakenRegex.Match(lines[j]);
                    if (cashTakenMatch.Success)
                    {
                        cashTakenTime = cashTakenMatch.Groups["CashTime"].Value;
                    }
                }

                if (!hasCard) continue;

                // Reference fallback
                if (string.IsNullOrEmpty(reference))
                {
                    reference = fallbackReference;
                }
                // Amount fallback
                if (string.IsNullOrEmpty(amount))
                {
                    amount = "0";
                }
                // Currency fallback
                if (string.IsNullOrEmpty(currency))
                {
                    currency = !string.IsNullOrEmpty(fallbackCurrency) ? fallbackCurrency : "KES";
                }
                // Status fallback
                if (string.IsNullOrEmpty(tranStat))
                {
                    tranStat = !string.IsNullOrEmpty(cashTakenTime) ? "APPROVED" : "DECLINED";
                }
                // Date fallback: always print if found
                string formattedDate = date;
                if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(startTime))
                {
                    if (DateTime.TryParseExact($"{date}:{startTime.Split(':')[2]}", "dd.MM.yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        formattedDate = dt.ToString("dd.MM.yy HH:mm");
                }

                // Write transaction details to CSV
                csv.AppendLine(string.Join(",",
                    cardNo,
                    formattedDate,
                    amount,
                    currency,
                    utrnNo,
                    tranStat,
                    rc,
                    authNo,
                    atmNo,
                    startTime,
                    transactionEnd,
                    reference,
                    cashTakenTime
                ));
            }
            // 1. Ensure we use the full, absolute path:

            var inputFull = Path.GetFullPath(inputFile);

            // 2. Get the directory of the .JRN file; 
            var inputDir = Path.GetDirectoryName(inputFull)
                ?? throw new InvalidOperationException($"Cannot determine directory of {inputFull}");


            // 3. Create a sibling "Conv" folder next to the .jrn file
            var parentDir = Directory.GetParent(inputDir)?.FullName
                ?? throw new InvalidOperationException($"Cannot determine parent directory of {inputDir}");

            var outDir = Path.Combine(parentDir, "Conv");
            Directory.CreateDirectory(outDir);

            // 4. Generate a safe suffix (last 14 chars of filename, no invalid chars or spaces)
            var rawName = Path.GetFileNameWithoutExtension(inputFull);
            var last14 = rawName.Length > 14 ? rawName[^14..] : rawName;
            var invalid = Path.GetInvalidFileNameChars().Concat("\\/:*?\"<>| ").Distinct().ToArray();
            var safeSuffix = new string(last14.Where(c => !invalid.Contains(c)).ToArray()).Trim();
            if (safeSuffix.Length == 0) safeSuffix = "ATMJournal";

            // 5. Build the final CSV filename for Kenyan context
            var outFile = Path.Combine(
                outDir,
                $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_AtmJnrl_{entity}_{safeSuffix}.csv"
            );

            // 6. Write it out (inside your existing try/catch)
            File.WriteAllText(outFile, csv.ToString());



        }
    }
}
