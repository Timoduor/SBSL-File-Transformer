using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using CsvHelper;

using SbslFileTransformer.Converters.Kenya.Models;

namespace SbslFileTransformer.Converters.Kenya
{
    public class NCR_JournalsConverter
    {
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            string[] lines = File.ReadAllLines(inputFile);

            List<NCR_Item> records = new List<NCR_Item>();

            NCR_Item record = null;

            foreach (var line in lines)
            {
                if (line.Contains("TRANSACTION STARTED"))
                    record = new NCR_Item();

                if (record != null)
                {
                    if (Regex.IsMatch(line, "^0{2} [0-9]{6}..[0-9]{4}$"))
                    {
                        record.CARD_NO = line.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
                    }

                    if (Regex.IsMatch(line, "^[0-9]{8} [0-9]{2}.[0-9]{2}.[0-9]{2} [0-9]{2}:[0-9]{2}$"))
                    {
                        string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        record.ATM_NO = parts[0];
                        record.DATE = parts[1] + " " + parts[2];
                    }

                    if (Regex.IsMatch(line, "^[A-Z0-9]{6}.[0-9]{12}[ ]{1,9}.[0-9]{1,10}.[0-9]{2} [A-Z]{3}$"))
                    {
                        string[] parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        record.REFERENCE = parts[0];
                        record.AMOUNT = parts[1];
                        record.CURRENCY = parts[2];
                    }

                    if (line.ToUpper().Contains("NOTES TAKEN"))
                    {
                        record.CASH_TAKEN = line.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
                    }

                    if (line.ToUpper().Contains("TRANSACTION END"))
                    {
                        record.TRANSACTION_END = line.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                }

                if (record != null &&
                !AnyIsNull(record.DATE, record.TRANSACTION_END, record.CASH_TAKEN, record.CURRENCY, record.CARD_NO, record.AMOUNT, record.ATM_NO, record.REFERENCE))
                {
                    records.Add(record);
                    record = new NCR_Item();
                }
            }

            if (records.Count() <= 0)
            {
                return;
            }


            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Directory.GetParent(Path.GetDirectoryName(inputFile)).ToString(), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{fileName}.csv");
            }

            WriteToFile(records, outputFile);
        }

        private bool AnyIsNull(params string[] args)
        {
            if (args.Any(string.IsNullOrEmpty))
                return true;

            return false;
        }

        private static void WriteToFile(List<NCR_Item> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<NCR_Item>();
                    csv.NextRecord();

                    foreach (NCR_Item row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}
