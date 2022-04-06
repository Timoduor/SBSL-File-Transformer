using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CsvHelper;
using SbslFileTransformer.Infrastructure.Helpers;

namespace SbslFileTransformer.Converters.Kenya.KenSwitch
{
    public class KenSwitchConverter
    {
        public void ConverterKenSwitchFile(string inputFile, string outputFolder = null)
        {
            if (string.IsNullOrEmpty(outputFolder))
                outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");

            string text = SbslPdfReader.GetTextFromPDF(inputFile);

            string[] lines = text.Split('\n', '\r');

            List<KenSwitchRec> outputLines = new List<KenSwitchRec>();

            KenSwitchRec rec = null;

            KenSwitchFileType ksType = KenSwitchFileType.ClientDebitActivity;

            string AcquirerIssuer = string.Empty;
            string TerminalId = string.Empty;
            string NameLocation = string.Empty;

            if (lines.Any(l => l.Contains("Client Debit Activity", StringComparison.OrdinalIgnoreCase)))
                ksType = KenSwitchFileType.ClientDebitActivity;

            if (lines.Any(l => l.Contains("ATM Activity", StringComparison.OrdinalIgnoreCase)))
                ksType = KenSwitchFileType.ATMActivity;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Acquirer:", StringComparison.OrdinalIgnoreCase) ||
                    lines[i].Contains("Issuer:", StringComparison.OrdinalIgnoreCase))
                {
                    if (lines[i].Split(" ").Length > 1)
                        AcquirerIssuer = lines[i].Split(" ")[1];
                    else
                        AcquirerIssuer = lines[i - 1];
                }

                if (lines[i].Contains("Terminal Id", StringComparison.OrdinalIgnoreCase))
                {
                    if (lines[i].Split(" ").Length > 6)
                    {
                        TerminalId = lines[i].Split(' ')[2];

                        if (ksType == KenSwitchFileType.ATMActivity)
                        {
                            NameLocation = lines[i].Split(':')[2];
                        }
                        else
                        {
                            IEnumerable<string> sec = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().Skip(6);

                            NameLocation = string.Join(" ", sec);
                        }
                    }
                    else
                    {
                        TerminalId = lines[i - 1].Split(' ')[0];

                        IEnumerable<string> sec = lines[i - 1].Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().Skip(1);

                        NameLocation = string.Join(" ", sec);
                    }
                }

                if (Regex.IsMatch(lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries)[0],
                    @"^\d{2}\/\d{2}\/\d{4}$") && lines[i].Split(" ").Length >= 7)
                {
                    rec = new KenSwitchRec();

                    rec.AcquirerIssuer = AcquirerIssuer;
                    rec.TerminalId = TerminalId;
                    rec.NameLocation = NameLocation;

                    string[] parts = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    switch (ksType)
                    {
                        case KenSwitchFileType.ClientDebitActivity:
                            if (parts.Count() == 6)
                            {
                                rec.Date = parts[0];
                                rec.Time = parts[1];
                                rec.CardNo = parts[2];
                                //rec.FromAcc = parts[3];
                                //rec.ToAcc = parts[4];
                                //rec.RRN1 = parts[3];
                                rec.RRN2 = parts[3];
                                rec.Stip = parts[4];
                                //rec.PartRev = parts[8];
                                rec.Amount = parts[5];
                            }

                            if (parts.Count() == 7)
                            {
                                rec.Date = parts[0];
                                rec.Time = parts[1];
                                rec.CardNo = parts[2];
                                //rec.FromAcc = parts[3];
                                //rec.ToAcc = parts[4];

                                if (parts[4] == "No")
                                {
                                    rec.RRN2 = parts[3];
                                    rec.Stip = parts[4];
                                    rec.PartRev = parts[5];
                                }
                                else
                                {
                                    rec.RRN1 = parts[3];
                                    rec.RRN2 = parts[4];
                                    rec.Stip = parts[5];
                                }


                                rec.Stip = parts[5];
                                //rec.PartRev = parts[8];
                                rec.Amount = parts[6];
                            }

                            if (parts.Count() == 8)
                            {
                                rec.Date = parts[0];
                                rec.Time = parts[1];
                                rec.CardNo = parts[2];
                                //rec.FromAcc = parts[3];
                                //rec.ToAcc = parts[4];
                                rec.RRN1 = parts[3];
                                rec.RRN2 = parts[4];
                                rec.Stip = parts[5];
                                rec.PartRev = parts[6];
                                rec.Amount = parts[7];
                            }

                            break;
                        case KenSwitchFileType.ATMActivity:
                            if (parts.Count() == 6)
                            {
                                rec.Date = parts[0];
                                rec.Time = parts[1];
                                rec.CardNo = parts[2];
                                //rec.FromAcc = parts[3];
                                //rec.ToAcc = parts[4];
                                rec.RRN1 = parts[3];
                                //rec.RRN2 = parts[6];
                                rec.Stip = parts[4];
                                //rec.PartRev = parts[8];
                                rec.Amount = parts[5];
                            }

                            if (parts.Count() == 7)
                            {
                                rec.Date = parts[0];
                                rec.Time = parts[1];
                                rec.CardNo = parts[2];
                                rec.FromAcc = parts[3];
                                //rec.ToAcc = parts[4];
                                rec.RRN1 = parts[4];
                                //rec.RRN2 = parts[6];
                                rec.Stip = parts[5];
                                //rec.PartRev = parts[8];
                                rec.Amount = parts[6];
                            }

                            if (parts.Count() == 8)
                            {
                                rec.Date = parts[0];
                                rec.Time = parts[1];
                                rec.CardNo = parts[2];
                                rec.FromAcc = parts[3];
                                rec.ToAcc = parts[4];
                                rec.RRN1 = parts[5];
                                //rec.RRN2 = parts[6];
                                rec.Stip = parts[6];
                                //rec.PartRev = parts[7];
                                rec.Amount = parts[7];
                            }

                            break;
                    }

                    outputLines.Add(rec);
                }
            }

            Directory.CreateDirectory(outputFolder);

            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            WriteToFile(outputLines,
                Path.Combine(outputFolder,
                    $"{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}_KS_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.csv"));

            Thread.Sleep(1000);
        }

        private static void WriteToFile(List<KenSwitchRec> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<KenSwitchRec>();
                    csv.NextRecord();

                    foreach (KenSwitchRec row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}