using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


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

            List<string> csv = File.ReadLines(filePath) // not AllLines
              .Select((line, index) => index == 0
                 ? line + "0,00"
                 : line + "0,00")
              .ToList(); // we should write into the same file, that´s why we materialize

            File.WriteAllLines(outputFile, csv);
        }
    }
}
