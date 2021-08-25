using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Rwanda
{ 
    public class fc_dailyConverter
    {
        public fc_dailyConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        public static DateTime GetLastDayOfTheMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
        }

        public void ConvertFile(string inputFile)
        {
            var list = new List<ExcelCols>();
            string outputFile = "";
            string outputFolder = null;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
                outputFolder = outputFolder + "\\conv";

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

            }


      

            var list2 = new List<ExcelCols>();
           


            string scontent = "";
            string scontentl2 = "";
         
            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var tables = result.Tables;

                    var sheet1 = tables[0];
      

                    foreach (DataRow row in sheet1.Rows)
                    {
                        var excelCol = new ExcelCols();
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
                        excelCol.Col17 = row[18].ToString();
                         

                        list2.Add(excelCol);

                    }


                }
            
                for (var i = 0; i < list2.Count - 1; i++)
                {
                    if (scontentl2 == "")
                    {
                        if ((list2[i].Col0.Trim() != "") && (list2[i].Col0.Trim() != "NET POSITION") && (list2[i].Col0.Trim() != "OPENING POSITION") && (list2[i].Col0.Trim() != "OPENING BALANCE") && (list2[i].Col0.Trim() != "TOTAL P/L"))
                            scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col3 + "," + list2[i].Col4 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col15 + "," + list2[i].Col16 + "," + list2[i].Col17 + "," + list2[i].Col18  + Environment.NewLine;

                    }
                    else
                    {
                        if ((list2[i].Col0.Trim() != "") && (list2[i].Col0.Trim() != "NET POSITION") && (list2[i].Col0.Trim() != "OPENING POSITION") && (list2[i].Col0.Trim() != "OPENING BALANCE") && (list2[i].Col0.Trim() != "TOTAL P/L"))
                            scontentl2 += list2[i].Col0.Trim() + "," + list2[i].Col1 + "," + list2[i].Col2 + "," + list2[i].Col3 + "," + list2[i].Col4 + "," + list2[i].Col5 + "," + list2[i].Col6 + "," + list2[i].Col7 + "," + list2[i].Col8 + "," + list2[i].Col9 + "," + list2[i].Col10 + "," + list2[i].Col11 + "," + list2[i].Col12 + "," + list2[i].Col13 + "," + list2[i].Col14 + "," + list2[i].Col15 + "," + list2[i].Col16 + "," + list2[i].Col17 + "," + list2[i].Col18 +  Environment.NewLine;
                    }
                }

                scontent = scontentl2;
                outputFile = outputFolder + "\\Converted_FC_DAILY_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv";

                WriteFile(outputFile, scontent);
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t"
                };

                using (var csv = new CsvWriter(writer, config))
                {
                    foreach (var row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
        public class ExcelCols
        {
            public string Col0 { get; set; }
            public string Col1 { get; set; }
            public string Col2 { get; set; }
            public string Col3 { get; set; }
            public string Col4 { get; set; }
            public string Col5 { get; set; }
            public string Col6 { get; set; }
            public string Col7 { get; set; }
            public string Col8 { get; set; }
            public string Col9 { get; set; }
            public string Col10 { get; set; }
            public string Col11 { get; set; }
            public string Col12 { get; set; }
            public string Col13 { get; set; }
            public string Col14 { get; set; }
            public string Col15 { get; set; }
            public string Col16 { get; set; }
            public string Col17 { get; set; }

            public string Col18{ get; set; }

        }
    }
}
