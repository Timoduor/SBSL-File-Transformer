using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class RWExcelToCSVConverter
    {
        private readonly ILogger<RWExcelToCSVConverter> _logger;

        public RWExcelToCSVConverter(ILogger<RWExcelToCSVConverter> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Converts all Excel files in the input folder to CSV files in the output folder.
        /// Returns a list of all converted CSV file paths.
        /// </summary>
        public async Task<List<string>> ConvertExcelFilesAsync(string inputFolder, string outputFolder = null)
        {
            var convertedFiles = new List<string>();

            if (!Directory.Exists(inputFolder))
                Directory.CreateDirectory(inputFolder);

            outputFolder ??= Path.Combine(inputFolder, "Converted");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var excelFiles = Directory.GetFiles(inputFolder, "*.xlsx")
                .Concat(Directory.GetFiles(inputFolder, "*.xls"))
                .ToList();

            if (!excelFiles.Any())
            {
                _logger.LogInformation($"No Excel files found in {inputFolder}");
                return convertedFiles;
            }

            foreach (var filePath in excelFiles)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string csvPath = Path.Combine(outputFolder, $"{fileName}.csv");

                    IWorkbook workbook;
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (Path.GetExtension(filePath).Equals(".xls", StringComparison.OrdinalIgnoreCase))
                            workbook = new HSSFWorkbook(fs);
                        else
                            workbook = new XSSFWorkbook(fs);
                    }

                    var sheet = workbook.GetSheetAt(0);
                    using (var writer = new StreamWriter(csvPath))
                    {
                        for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
                        {
                            var row = sheet.GetRow(i);
                            if (row == null) continue;
                            var values = new string[row.LastCellNum];
                            for (int j = 0; j < row.LastCellNum; j++)
                            {
                                var cell = row.GetCell(j);
                                values[j] = cell == null ? "" : $"\"{cell.ToString().Replace("\"", "\"\"")}\"";
                            }
                            writer.WriteLine(string.Join(",", values));
                        }
                    }

                    convertedFiles.Add(csvPath);
                    _logger.LogInformation($"Converted {fileName} → {csvPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error converting file: {filePath}");
                }
            }

            return convertedFiles;
        }
    }
}
