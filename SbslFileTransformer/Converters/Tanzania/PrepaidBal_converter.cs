using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class PrepaidBal_converter
    {

        public PrepaidBal_converter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public static bool IsNumeric(object Expression)
        {

            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out double retNum);
            return isNum;
        }
        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        public void ConvertFile(string inputFile, string rootfolder = "")
        {

            string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
            string fileOut = outputFolder + "\\Conv_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv";
            string fileIn = inputFile;
            double USDbal = 0;
            double GBPbal = 0;
            double EURbal = 0;
            double INRbal = 0;
            double TZSbal = 0;
            string scontent = "";
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);
            StringBuilder toAppend = new StringBuilder();

            string[] lines = System.IO.File.ReadAllLines(fileIn);
            List<string> newLines = new List<string>();
            DateTime FileDate_ = Convert.ToDateTime(lines[0].Split(',')[11]);
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");
            string outputFile = Path.Combine(rootfolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_prepaidbal_{"IMTZ"}.txt");


            for (int i = 5; i < lines.Length; i++)
            {
                string temp = lines[i];

                string[] fields = temp.Split(',');


                if (lines[i].Split(',').Length > 13)
                {
                    if (lines[i].Split(',')[2].ToString() != "")
                    {
                        if (IsNumeric(lines[i].Split(',')[4].ToString()) == true)
                        {
                            USDbal = USDbal + Convert.ToDouble(lines[i].Split(',')[6]);
                            GBPbal = GBPbal + Convert.ToDouble(lines[i].Split(',')[7]);
                            EURbal = EURbal + Convert.ToDouble(lines[i].Split(',')[8]);
                            INRbal = INRbal + Convert.ToDouble(lines[i].Split(',')[10]);
                            TZSbal = TZSbal + Convert.ToDouble(lines[i].Split(',')[13]);


                        }
                    }

                }

            }
            scontent = $"IMTZ\t{"3099430020014"}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(FileDate_):MM-dd-yyyy}\t\t\t\t{decimal.Round(Convert.ToDecimal(USDbal), 2)}\t{"USD"}\n";
            scontent += $"IMTZ\t{"3099430010019"}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(FileDate_):MM-dd-yyyy}\t\t\t\t{decimal.Round(Convert.ToDecimal(TZSbal), 2)}\t{"TZS"}\n";
            scontent += $"IMTZ\t{"3099430020015"}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(FileDate_):MM-dd-yyyy}\t\t\t\t{decimal.Round(Convert.ToDecimal(GBPbal), 2)}\t{"GBP"}\n";
            scontent += $"IMTZ\t{"3099430020016"}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(FileDate_):MM-dd-yyyy}\t\t\t\t{decimal.Round(Convert.ToDecimal(EURbal), 2)}\t{"EUR"}\n";
            scontent += $"IMTZ\t{"3099430020017"}\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(FileDate_):MM-dd-yyyy}\t\t\t\t{decimal.Round(Convert.ToDecimal(INRbal), 2)}\t{"INR"}\n";

            WriteFile(outputFile, scontent);
        }

    }
}
