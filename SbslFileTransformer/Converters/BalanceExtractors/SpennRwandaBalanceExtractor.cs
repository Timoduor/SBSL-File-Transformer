using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters.BalanceExtractors
{
    public class SpennRwandaBalanceExtractor
    {
        string _entity;
        public SpennRwandaBalanceExtractor(string entity)
        {
            _entity = entity;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string rootFolder)
        {
            
        }

        private void GenerateMultiCurr(List<ExcelCols> list, string inputFile, string rootFolder)
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");

            var outputFile = Path.Combine(rootFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_FDI_{_entity}.txt");

            var toAppend = new StringBuilder();

            DateTime date = Convert.ToDateTime(list.First().Col0);
            var amount = list.First().Col3; //vs col5 diff
            var currency = "RWF";

            toAppend.Append($"{_entity}\t20100243506073\tMobile Banking\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(date):MM/dd/yyyy}\t\t\t\t{amount}\t{currency}\n");

            var text = toAppend.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                File.WriteAllText(outputFile, text);
            }
        }

    }
}
