using SbslFileTransformer.Infrastructure.Helpers;
using System;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class FxPosGLBalConverter
    {
        public FxPosGLBalConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        public void ConvertFile(string inputFile, string outputFolder = "")
        {
            string content = File.ReadAllText(inputFile);
            string[] sDet = File.ReadAllLines(inputFile);

            string toAppend = "";
            DateTime baldate = DateTime.Now;
            string[] sGrp = content.Split("\n");

            for (int i = 0; i < sGrp.Length - 1; i++)
            {
                //acc sGrp[i].Split(',')[3] GL Code sGrp[i].Split(',')[2] acc name sGrp[i].Split(',')[4] Curr sGrp[i].Split(',')[1] bal c
                if (toAppend == "")
                {
                    toAppend = $"{"IMKE"}\t{sGrp[i].Split(',')[3].Trim()}{"  FX Pos Kenya"}\t{sGrp[i].Split(',')[2].Trim()}\t\t\t\t\t\t\t{sGrp[i].Split(',')[4].Trim()}{" "}{sGrp[i].Split(',')[1].Trim()}\t{"FX Pos Kenya"}\t{"A"}\t{"Asset"}\t{"TRUE"}\t{"TRUE"}\t\t{sGrp[i].Split(',')[1]}\t{ContentHelpers.GetLastDayOfTheMonth(DateTime.Now):MM-dd-yyyy}\t\t\t{Convert.ToDouble(sGrp[i].Split(',')[7]) + Convert.ToDouble(sGrp[i].Split(',')[9])}\n";
                }
                else
                {
                    toAppend += $"{"IMKE"}\t{sGrp[i].Split(',')[3].Trim()}{"  FX Pos Kenya"}\t{sGrp[i].Split(',')[2].Trim()}\t\t\t\t\t\t\t{sGrp[i].Split(',')[4].Trim()}{" "}{sGrp[i].Split(',')[1].Trim()}\t{"FX Pos Kenya"}\t{"A"}\t{"Asset"}\t{"TRUE"}\t{"TRUE"}\t\t{sGrp[i].Split(',')[1]}\t{ContentHelpers.GetLastDayOfTheMonth(DateTime.Now):MM-dd-yyyy}\t\t\t{Convert.ToDouble(sGrp[i].Split(',')[7]) + Convert.ToDouble(sGrp[i].Split(',')[9])}\n";
                }

            }
            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");
            string outputFileGL = Path.Combine(outputFolder, $"GLAccounts_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_FXPos_{"IMKE"}.txt");
            WriteFile(outputFileGL, toAppend);
        }


    }
}

