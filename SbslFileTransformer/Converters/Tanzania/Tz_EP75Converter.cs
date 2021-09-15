using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SbslFileTransformer.Converters
{
    public class Tz_EP75Converter
    {

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var lines = File.ReadAllLines(inputFile);

            var records = new List<EP75Item>();

            for (var i = 0; i < lines.Length; i++)
            {
                var batch = lines[i].Substring(0, 4).Trim();

                var cardno = lines[i].Substring(20, 20).Trim();

                if (int.TryParse(batch, out var batchNio) && cardno.Length == 16)
                {
                    var amount = lines[i].Substring(101, 13).Trim();

                    var rec = new EP75Item
                    {
                        BatchNo = lines[i].Substring(0, 4).Trim(),
                        TranDate = lines[i].Substring(5, 6).Trim(),
                        TranTime = lines[i].Substring(11, 9).Trim(),
                        CardNo = lines[i].Substring(20, 20).Trim(),
                        ReferenceNo = lines[i].Substring(40, 13).Trim(),
                        TraceNo = lines[i].Substring(53, 7).Trim(),
                        IssuerDetails = lines[i].Substring(60, 12).Trim(),
                        TranType = lines[i].Substring(72, 5).Trim(),
                        ProcessCode = lines[i].Substring(77, 7).Trim(),
                        EntryMode = lines[i].Substring(84, 9).Trim(),
                        ReasonCode = lines[i].Substring(93, 4).Trim(),
                        RspCode = lines[i].Substring(97, 4).Trim(),
                        TranAmount = decimal.Parse(amount),
                        Currency = lines[i].Substring(114, 4).Trim(),
                        SettledAmount = lines[i].Substring(118, 15).Trim(),
                        Terminal = lines[i + 1].Substring(63, 22).Trim()
                    };

                    records.Add(rec);
                }
            }


            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_Ep75_{fileName.Substring(Math.Max(0, fileName.Length - 10))}.csv");
            }

            WriteToFile(records, outputFile);
        }

        private static void WriteToFile(List<EP75Item> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<EP75Item>();
                    csv.NextRecord();

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