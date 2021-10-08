using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SbslFileTransformer.Infrastructure.Helpers;


using Microsoft.Extensions.DependencyInjection;
using SbslFileTransformer.Data;
using System.Linq;
using System.Threading.Tasks;

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
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        public void ConvertFile(string inputFile, string outputFolder = "")
        {
            var content = File.ReadAllText(inputFile);
            var sDet = File.ReadAllLines(inputFile);
            string outputFile = "";
           
            var toAppend = "";
            DateTime baldate = DateTime.Now;
            string[] sGrp = content.Split("\n");

            for (var i = 0; i < sGrp.Length - 1; i++)
            {
                //acc sGrp[i].Split(',')[3] GL Code sGrp[i].Split(',')[2] acc name sGrp[i].Split(',')[4] Curr sGrp[i].Split(',')[1] bal c
                if (toAppend=="")
                {
                    toAppend = $"{"IMKE"}\t{sGrp[i].Split(',')[3].Trim()}{"  FX Pos Kenya"}\t{sGrp[i].Split(',')[2].Trim()}\t\t\t\t\t\t\t{sGrp[i].Split(',')[4].Trim()}{" "}{sGrp[i].Split(',')[1].Trim()}\t{"FX Pos Kenya"}\t{"A"}\t{"Asset"}\t{"TRUE"}\t{"TRUE"}\t\t{sGrp[i].Split(',')[1]}\t{ContentHelpers.GetLastDayOfTheMonth(DateTime.Now):MM-dd-yyyy}\t\t\t{Convert.ToDouble(sGrp[i].Split(',')[7]) + Convert.ToDouble(sGrp[i].Split(',')[9])}\n";
                }
                else
                {
                    toAppend += $"{"IMKE"}\t{sGrp[i].Split(',')[3].Trim()}{"  FX Pos Kenya"}\t{sGrp[i].Split(',')[2].Trim()}\t\t\t\t\t\t\t{sGrp[i].Split(',')[4].Trim()}{" "}{sGrp[i].Split(',')[1].Trim()}\t{"FX Pos Kenya"}\t{"A"}\t{"Asset"}\t{"TRUE"}\t{"TRUE"}\t\t{sGrp[i].Split(',')[1]}\t{ContentHelpers.GetLastDayOfTheMonth(DateTime.Now):MM-dd-yyyy}\t\t\t{Convert.ToDouble(sGrp[i].Split(',')[7]) + Convert.ToDouble(sGrp[i].Split(',')[9])}\n";
                }
              
            }
            var fileName = Path.GetFileNameWithoutExtension(inputFile);

            var fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 13)).Replace(" ", "");
            var outputFileGL = Path.Combine(outputFolder, $"GLAccounts_{DateTime.Now:yyyy_MM_dd}_{fileNameToAppend}_FXPos_{"IMKE"}.txt");
          WriteFile(outputFileGL, toAppend);
        }

            
     }
    }
 
