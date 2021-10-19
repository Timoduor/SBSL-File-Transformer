using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SbslFileTransformer.Infrastructure.Helpers; 
using System.Linq;


namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class NotonusPOSConverter
    {

        public NotonusPOSConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void RW_Add_Extracol(string inputFile, string outputFile)
        {
            String filePath = inputFile;
            string outputDirectory = "";
            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.GetDirectoryName(inputFile);
            }
            outputDirectory = Path.GetDirectoryName(inputFile) + "\\Conv";
              outputFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(inputFile) + ".csv");
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var csv = File.ReadLines(filePath) // not AllLines
              .Select((line, index) => index == 0
                 ? line + "0,00"
                 : line + "0,00")
              .ToList(); // we should write into the same file, that´s why we materialize

            File.WriteAllLines(outputFile, csv);
        }
    }
}
