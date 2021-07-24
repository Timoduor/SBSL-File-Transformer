using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SbslFileTransformer.Converters
{
    public class MasterCardConverter
    {
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var lines = File.ReadAllLines(inputFile);

            var records = new List<MasterCardResult>();

            foreach (var line in lines)
            {
                if (!line.StartsWith("FREC") && !line.StartsWith("NREC")) continue;

                var record = new MasterCardResult
                {
                    TransactionType = line.Substring(0, 4),
                    Code = line.Substring(4, 14),
                    Date = line.Substring(18, 6),
                    StanNo = line.Substring(24, 6),
                    CardNo = line.Substring(32, 15) + "0",
                    SNo1 = line.Substring(57, 6),
                    SNo2 = line.Substring(63, 7),
                    TerminalId = line.Substring(92, 8),
                    AmountDispensed = $"{line.Substring(132, 6)}.{line.Substring(138, 2)}",
                    DorC = line.Substring(140, 1),
                    AmountPaid = $"{line.Substring(177, 8)}.{line.Substring(185, 2)}",
                    DorC2 = line.Substring(187, 1),
                    Fees = $"{line.Substring(188, 8)}.{line.Substring(196, 2)}",
                    DorC3 = line.Substring(198, 1)
                };

                records.Add(record);
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var name = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MC_{name.Substring(Math.Max(0, name.Length - 10))}.csv");
            }


            WriteToFile(records, outputFile);
        }

        private void WriteToFile(List<MasterCardResult> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<MasterCardResult>();
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