using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ExcelDataReader;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class KE_Debtorslist
    {
        public void ConvertFile(string inputFile,string rootFolder=null)
        {
            List<DlCols> list = new List<DlCols>();
            string outputFolder = "";
            double runnbal = 0;
            string scontent = "";
            string dupContent = "";

            using (FileStream stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;
                if (Path.GetExtension(inputFile).ToLower().Contains("csv"))
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                else
                    reader = ExcelReaderFactory.CreateReader(stream);

                using (reader)
                {
                    while (reader.Read())
                    {
                        DlCols row = new DlCols();

                   



                        row.Col1 = "18000113002067";

                      
                        var r = new DlCols
                        {
                            Col0 = reader.GetValue(0)?.ToString(),

                            Col1 = reader.GetValue(1)?.ToString(),

                            Col2 = reader.GetValue(2)?.ToString(),

                            Col3 = reader.GetValue(3)?.ToString(),

                            Col4 = reader.GetValue(4)?.ToString(),

                            Col5 = reader.GetValue(5)?.ToString(),

                            Col6 = reader.GetValue(6)?.ToString(),

                            Col7 = reader.GetValue(7)?.ToString(),

                            Col8 = reader.GetValue(8)?.ToString(),

                            Col9 = reader.GetValue(9)?.ToString(),

                            Col10 = reader.GetValue(10)?.ToString(),

                            Col11 = reader.GetValue(11)?.ToString(),

                            Col12 = reader.GetValue(12)?.ToString(),

                            Col13 = reader.GetValue(13)?.ToString(),

                            Col14 = reader.GetValue(14)?.ToString(),

                            Col15 = reader.GetValue(15)?.ToString(),
                        };



                        if (reader.GetValue(4) != null)
                        {

                            if (scontent.Contains(reader.GetValue(1).ToString().Trim()))
                            {
                                dupContent += " " + reader.GetValue(1).ToString().Trim() + " " + Environment.NewLine;
                            }
                            else
                            {
                                if (reader.GetValue(4).ToString().ToUpper() != "TOTAL_BALANCE")
                                {
                                    runnbal = runnbal + Convert.ToDouble(reader.GetValue(4)?.ToString());
                                }

                                scontent += reader.GetValue(0).ToString().Trim() + "," + reader.GetValue(1).ToString().Trim() + "," + reader.GetValue(2).ToString().Trim() + "," + reader.GetValue(3).ToString().Trim() + "," + reader.GetValue(4).ToString().Trim() + "," + reader.GetValue(5).ToString().Trim() + "," +
                                     " " + reader.GetValue(6).ToString().Trim() + "," + reader.GetValue(7).ToString().Trim() + "," + reader.GetValue(8).ToString().Trim() + "," + reader.GetValue(9).ToString().Trim() + "," + reader.GetValue(10).ToString().Trim() + "," +
                                    " " + reader.GetValue(11).ToString().Trim() + "," + reader.GetValue(12).ToString().Trim() + "," + reader.GetValue(13).ToString().Trim() + "," + reader.GetValue(14).ToString().Trim() + "," + reader.GetValue(15).ToString().Trim() + " " + Environment.NewLine;

                                list.Add(r);
                            }



                        }

                    }
                }
            }

            outputFolder = Path.GetDirectoryName(inputFile);

            outputFolder = outputFolder  + "\\Conv";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            if (list.Count > 0)
            {


                if (runnbal > 0)
                {
                    runnbal = runnbal * -1;
                }

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                string fileNameToAppend = fileName.Substring(Math.Max(0, fileName.Length - 20)).Replace(" ", "");
                string outputFile = Path.Combine(rootFolder,
                    $"Multicurr_{DateTime.Now:dd_MM_yyyy}_{fileNameToAppend}_DebtorsCards.txt");
                DlCols firstRow = list.OrderByDescending(i => i.ReconDate)
                    .FirstOrDefault(c => c.ReconDate == list.Max(r => r.ReconDate));



                string toAppend =
                   $"IMKE\t{18000113002067}\tCards Kenya\t\t\t\t\t\t\t\t\t\tBalance_bank\t{ContentHelpers.GetLastDayOfTheMonth(DateTime.Today):MM/dd/yyyy}\t\t\t\t{runnbal}\tKES\n";
                if (!string.IsNullOrEmpty(toAppend)) File.WriteAllText(outputFile, toAppend);

                if (!string.IsNullOrEmpty(list.ToString())) File.WriteAllText(Path.GetDirectoryName(inputFile) + "\\conv\\conv_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv", scontent);


                //if (!string.IsNullOrEmpty(list.ToString())) File.WriteAllText(Path.GetDirectoryName(inputFile) + "\\conv\\" + Path.GetFileNameWithoutExtension(inputFile) + "_dups.csv", dupContent);
            }
        }




        public static class ContentHelpers
        {
            public static DateTime GetLastDayOfTheMonth(DateTime date2)
            {
                return new DateTime(date2.Year, date2.Month, 1).AddMonths(1).AddDays(-1);
            }
        }


        public class DlCols
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

            public DateTime ReconDate { get; set; }
   
        }
    }
}
