using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class SaveFiles
    {
        public static void SaveToCsv(List<ExtractedRecord> Ntry, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Configuration.Delimiter = ",";
                csvWriter.Configuration.AutoMap<ExtractedRecord>();

                csvWriter.WriteRecords(Ntry);
            }
        }

        public static void BalanceToCSV(List<BalanceExctracted> Bal, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Configuration.Delimiter = ",";
                csvWriter.Configuration.AutoMap<BalanceExctracted>();

                csvWriter.WriteRecords(Bal);
            }
        }
    }
}
