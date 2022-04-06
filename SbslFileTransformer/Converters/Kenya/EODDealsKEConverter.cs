using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using SbslFileTransformer.Converters.Rwanda.BNR;


namespace SbslFileTransformer.Converters.Kenya
{
    public class EODDealsKEConverter
    {
        public EODDealsKEConverter()
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
        public void POSTEOD_ConvertFile(string inputFile)
        {

            List<ExcelCols> list2 = new List<ExcelCols>();
            List<ExcelCols> list3 = new List<ExcelCols>();
            List<ExcelCols> list4 = new List<ExcelCols>();
            List<ExcelCols> list5 = new List<ExcelCols>();
            List<ExcelCols> list6 = new List<ExcelCols>();
            List<ExcelCols> list7 = new List<ExcelCols>();
            List<ExcelCols> list8 = new List<ExcelCols>();
            List<ExcelCols> list9 = new List<ExcelCols>();
            List<ExcelCols> list10 = new List<ExcelCols>();
            List<ExcelCols> list11 = new List<ExcelCols>();
            List<ExcelCols> list12 = new List<ExcelCols>();
            List<ExcelCols> list13 = new List<ExcelCols>();

            string outputFolder = null;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
            }
            outputFolder = outputFolder + "\\Conv";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string firstdate = "";
            string lastdate = "";

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
                        excelCol.Col14 = row[14].ToString();
                        excelCol.Col15 = row[15].ToString();
                        excelCol.Col16 = row[16].ToString();
                        excelCol.Col17 = row[17].ToString();



                        list2.Add(excelCol);

                    }

                }
            }
            //Sheet 1
            List<ExcelCols> l = list2;

            for (int i = 1; i < list2.Count - 1; i++)
            {
                if (list2[i].Col17.Trim() != "Capture Timestamp")
                {
                    if (firstdate == "")
                    {

                        firstdate = Convert.ToDateTime(list2[i].Col17.Trim()).Date.ToString("M/dd/yyyy");
                    }
                    else
                    {
                        if (firstdate == Convert.ToDateTime(list2[i].Col17.Trim()).Date.ToString("M/dd/yyyy"))
                        {

                        }
                        else
                        {
                            if (Convert.ToDateTime(firstdate) > Convert.ToDateTime(list2[i].Col17.Trim()).Date)
                            {
                                lastdate = firstdate;


                            }
                            else
                            { lastdate = Convert.ToDateTime(list2[i].Col17.Trim()).Date.ToString("M/dd/yyyy"); }

                        }

                    }

                }



            }

            for (int i = 0; i < list2.Count - 1; i++)
            {
                if (scontentl2 == "")
                {

                    scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col15 + "," + list2[i].Col16 + "," + list2[i].Col17 + Environment.NewLine;


                }
                else
                {
                    if (Convert.ToDateTime(list2[i].Col17.Trim()).Date.ToString("M/dd/yyyy") == lastdate)
                    {
                        scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col15 + "," + list2[i].Col16 + "," + list2[i].Col17 + Environment.NewLine;
                    }
                }
            }

            scontent = scontentl2;

            WriteFile(outputFolder + "\\Converted_EOD_DEAL_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv", scontent);
        }

    }
}
