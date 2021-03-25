using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SbslFileTransformer.Converters.Camt053
{
    public class SaveFiles
    {
        public static void SaveToCsv(List<ExtractedRecord> Ntry, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Configuration.Delimiter = ",";
                csvWriter.Configuration.AutoMap<ExtractedRecord>();

                csvWriter.WriteRecords(Ntry);
            }
        }

        public static void BalanceToCSV(List<BalanceExctracted> Bal, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Configuration.Delimiter = ",";
                csvWriter.Configuration.AutoMap<BalanceExctracted>();

                csvWriter.WriteRecords(Bal);
            }

        }
    }
}
