using CsvHelper;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace TransactionReportDec5th
{
    public class MasterCardRWConverter
    {
        public MasterCardRWConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            using (var stream = File.Open(inputFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {

                        var row = new ExcelCols
                        {
                            //Reference
                            Col0 = reader.GetValue(0)?.ToString().Replace("\n", ""),

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

                            Col16 = reader.GetValue(16)?.ToString(),

                            Col17 = reader.GetValue(17)?.ToString(),

                            Col18 = reader.GetValue(18)?.ToString(),

                            Col19 = reader.GetValue(19)?.ToString(),

                            Col20 = reader.GetValue(20)?.ToString(),

                            Col21 = reader.GetValue(21)?.ToString(),

                            Col22 = reader.GetValue(22)?.ToString(),

                            Col23 = reader.GetValue(23)?.ToString(),

                            Col24 = reader.GetValue(24)?.ToString(),

                            Col25 = reader.GetValue(25)?.ToString(),

                            Col26 = reader.GetValue(26)?.ToString(),

                            Col27 = reader.GetValue(27)?.ToString(),

                            Col28 = reader.GetValue(28)?.ToString(),

                            Col29 = reader.GetValue(29)?.ToString(),

                            Col30 = reader.GetValue(30)?.ToString(),

                            Col31 = reader.GetValue(31)?.ToString(),

                            Col32 = reader.GetValue(32)?.ToString(),

                            Col33 = reader.GetValue(33)?.ToString(),

                            Col34 = reader.GetValue(34)?.ToString(),

                            Col35 = reader.GetValue(35)?.ToString(),

                            Col36 = reader.GetValue(36)?.ToString(),

                            Col37 = reader.GetValue(37)?.ToString(),

                            Col38 = reader.GetValue(38)?.ToString(),

                            Col39 = reader.GetValue(39)?.ToString(),

                            Col40 = reader.GetValue(40)?.ToString(),

                            Col41 = reader.GetValue(41)?.ToString(),

                            Col42 = reader.GetValue(42)?.ToString(),

                            Col43 = reader.GetValue(43)?.ToString(),

                            Col44 = reader.GetValue(44)?.ToString(),

                            Col45 = reader.GetValue(45)?.ToString(),

                            Col46 = reader.GetValue(46)?.ToString(),

                            Col47 = reader.GetValue(47)?.ToString(),

                            Col48 = reader.GetValue(48)?.ToString(),

                            Col49 = reader.GetValue(49)?.ToString(),

                            Col50 = reader.GetValue(50)?.ToString(),

                            Col51 = reader.GetValue(51)?.ToString(),

                            Col52 = reader.GetValue(52)?.ToString(),

                            Col53 = reader.GetValue(53)?.ToString(),

                            Col54 = reader.GetValue(54)?.ToString(),

                            Col55 = reader.GetValue(55)?.ToString(),

                            Col56 = reader.GetValue(56)?.ToString(),

                            Col57 = reader.GetValue(57)?.ToString(),

                            Col58 = reader.GetValue(58)?.ToString(),

                            Col59 = reader.GetValue(59)?.ToString(),

                            Col60 = reader.GetValue(60)?.ToString(),

                            Col61 = reader.GetValue(61)?.ToString(),

                            Col62 = reader.GetValue(62)?.ToString(),

                            Col63 = reader.GetValue(63)?.ToString(),

                            Col64 = reader.GetValue(64)?.ToString(),

                            Col65 = reader.GetValue(65)?.ToString(),

                            Col66 = reader.GetValue(66)?.ToString(),

                        };
                        list.Add(row);
                    }
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MasterCard_{fileName.Substring(Math.Max(0, fileName.Length - 20)).Replace(" ", "")}.csv");
            }

            WriteToFile(list, outputFile);
        }
        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (var writer = new StreamWriter(outputFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
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
            public string Col18 { get; set; }
            public string Col19 { get; set; }
            public string Col20 { get; set; }
            public string Col21 { get; set; }
            public string Col22 { get; set; }
            public string Col23 { get; set; }
            public string Col24 { get; set; }
            public string Col25 { get; set; }
            public string Col26 { get; set; }
            public string Col27 { get; set; }
            public string Col28 { get; set; }
            public string Col29 { get; set; }
            public string Col30 { get; set; }
            public string Col31 { get; set; }
            public string Col32 { get; set; }
            public string Col33 { get; set; }
            public string Col34 { get; set; }
            public string Col35 { get; set; }
            public string Col36 { get; set; }
            public string Col37 { get; set; }
            public string Col38 { get; set; }
            public string Col39 { get; set; }
            public string Col40 { get; set; }
            public string Col41 { get; set; }
            public string Col42 { get; set; }
            public string Col43 { get; set; }
            public string Col44 { get; set; }
            public string Col45 { get; set; }
            public string Col46 { get; set; }
            public string Col47 { get; set; }
            public string Col48 { get; set; }
            public string Col49 { get; set; }
            public string Col50 { get; set; }
            public string Col51 { get; set; }
            public string Col52 { get; set; }
            public string Col53 { get; set; }
            public string Col54 { get; set; }
            public string Col55 { get; set; }
            public string Col56 { get; set; }
            public string Col57 { get; set; }
            public string Col58 { get; set; }
            public string Col59 { get; set; }
            public string Col60 { get; set; }
            public string Col61 { get; set; }
            public string Col62 { get; set; }
            public string Col63 { get; set; }
            public string Col64 { get; set; }
            public string Col65 { get; set; }
            public string Col66 { get; set; }

        }
    }
}
