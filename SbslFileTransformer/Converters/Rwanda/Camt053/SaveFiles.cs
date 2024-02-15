using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class SaveFiles
    {
        public static void SaveToCsv(List<ExtractedRecord> Ntry, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," };

                using (CsvWriter csvWriter = new CsvWriter(writer, config))
                {
                    csvWriter.Context.AutoMap<ExtractedRecord>();

                    csvWriter.WriteRecords(Ntry);
                }
            }
        }

        public static void BalanceToCSV(List<BalanceExctracted> Bal, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "," };

                using (CsvWriter csvWriter = new CsvWriter(writer, config))
                {
                    csvWriter.Context.AutoMap<BalanceExctracted>();

                    csvWriter.WriteRecords(Bal);
                }
            }
        }
    }
}
