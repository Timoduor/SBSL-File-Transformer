using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class Ft_dailyConverter
    {
        public Ft_dailyConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        public static DateTime GetLastDayOfTheMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        public void ConvertFile(string inputFile, string outputFolder = "")
        {
            List<ExcelCols> list = new List<ExcelCols>();
            string outputFile = "";
            //string outputFolder = null;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
                outputFolder = outputFolder + "\\conv";

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

            }




            List<ExcelCols> list2 = new List<ExcelCols>();



            string scontent = "";
            string scontentl2 = "";

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet result = reader.AsDataSet();
                    DataTableCollection tables = result.Tables;

                    DataTable sheet1 = tables[0];


                    foreach (DataRow row in sheet1.Rows)
                    {
                        ExcelCols excelCol = new ExcelCols();
                        excelCol.Col0 = row[0].ToString();
                        excelCol.Col1 = row[1].ToString();
                        excelCol.Col2 = row[2].ToString();
                        excelCol.Col3 = row[3].ToString();
                        excelCol.Col4 = row[4].ToString();
                        excelCol.Col5 = row[5].ToString();
                        excelCol.Col6 = row[6].ToString();
                        excelCol.Col7 = row[7].ToString();
                        excelCol.Col8 = row[8].ToString();
                        excelCol.Col9 = row[9].ToString();
                        excelCol.Col10 = row[10].ToString();
                        excelCol.Col11 = row[11].ToString();
                        excelCol.Col12 = row[12].ToString();
                        excelCol.Col13 = row[13].ToString();
                        //excelCol.Col14 = row[14].ToString();
                        //excelCol.Col15 = row[15].ToString();
                        //excelCol.Col16 = row[16].ToString();
                        //excelCol.Col17 = row[17].ToString();
                        //excelCol.Col17 = row[18].ToString();


                        list2.Add(excelCol);

                    }


                }

                for (int i = 0; i < list2.Count - 1; i++)
                {
                    if (scontentl2 == "")
                    {

                        scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col3 + "," + list2[i].Col4 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + Environment.NewLine;

                    }
                    else
                    {

                        scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col3 + "," + list2[i].Col4 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + Environment.NewLine;
                    }
                }

                scontent = scontentl2;
                //outputFile = outputFolder + "\\Converted_FT_DAILY_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv";
                outputFile = Path.Combine(outputFolder, $"MultiCurr_{DateTime.Now:yyyy_MM_dd}_{Path.GetFileNameWithoutExtension(inputFile)}_fxdaily_{"IMRW"}.txt");
                WriteFile(outputFile, scontent);
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
    }


}
