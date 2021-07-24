using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SbslFileTransformer.Converters.Kenya
{
    public class WesternUnionSettlementRWConverter
    {
        public WesternUnionSettlementRWConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            var list = new List<ExcelCols>();

            int countHeader = 0;

            double per = 0.18;

            string vat = "VAT";

            string computedbaseamnt = "Computed Base Amount";

            string computedbaseamnt1 = "Computed Base Amount without decimal";

            var lines = File.ReadAllLines(inputFile);

            foreach (var line in lines)
            {
                var row = new ExcelCols();

                row.Col0 = line.Split("\t")[0].ToString();

                row.Col1 = line.Split("\t")[1].ToString();

                row.Col2 = line.Split("\t")[2].ToString();

                row.Col3 = line.Split("\t")[3].ToString();

                row.Col4 = line.Split("\t")[4].ToString();

                row.Col5 = line.Split("\t")[5].ToString();

                row.Col6 = line.Split("\t")[6].ToString();

                row.Col7 = line.Split("\t")[7].ToString();

                row.Col8 = line.Split("\t")[8].ToString();

                row.Col9 = line.Split("\t")[9].ToString();

                row.Col10 = line.Split("\t")[10].ToString();

                row.Col11 = line.Split("\t")[11].ToString();

                row.Col12 = line.Split("\t")[12].ToString();

                row.Col13 = line.Split("\t")[13].ToString();

                row.Col14 = line.Split("\t")[14].ToString();

                row.Col15 = line.Split("\t")[15].ToString();

                row.Col16 = line.Split("\t")[16].ToString();

                row.Col17 = line.Split("\t")[17].ToString();

                row.Col18 = line.Split("\t")[18].ToString();

                row.Col19 = line.Split("\t")[19].ToString();

                row.Col20 = line.Split("\t")[20].ToString();

                row.Col21 = line.Split("\t")[21].ToString();

                row.Col22 = line.Split("\t")[22].ToString();

                row.Col23 = line.Split("\t")[23].ToString();

                row.Col24 = line.Split("\t")[24].ToString();

                row.Col25 = line.Split("\t")[25].ToString();

                row.Col26 = line.Split("\t")[26].ToString();

                row.Col27 = line.Split("\t")[27].ToString();

                row.Col28 = line.Split("\t")[28].ToString();

                row.Col29 = line.Split("\t")[29].ToString();

                row.Col30 = line.Split("\t")[30].ToString();

                row.Col31 = line.Split("\t")[31].ToString();

                row.Col32 = line.Split("\t")[32].ToString();

                row.Col33 = line.Split("\t")[33].ToString();

                row.Col34 = line.Split("\t")[34].ToString();

                row.Col35 = line.Split("\t")[35].ToString();

                row.Col36 = line.Split("\t")[36].ToString();

                row.Col37 = line.Split("\t")[37].ToString();

                row.Col38 = line.Split("\t")[38].ToString();

                row.Col39 = line.Split("\t")[39].ToString();

                row.Col40 = line.Split("\t")[40].ToString();

                row.Col41 = line.Split("\t")[41].ToString();

                row.Col42 = line.Split("\t")[42].ToString();

                row.Col43 = line.Split("\t")[43].ToString();

                row.Col44 = line.Split("\t")[44].ToString();

                row.Col45 = line.Split("\t")[45].ToString();

                row.Col46 = line.Split("\t")[46].ToString();

                row.Col47 = line.Split("\t")[47].ToString();

                row.Col48 = line.Split("\t")[48].ToString();

                row.Col49 = line.Split("\t")[49].ToString();

                row.Col50 = line.Split("\t")[50].ToString();

                row.Col51 = line.Split("\t")[51].ToString();

                row.Col52 = line.Split("\t")[52].ToString();

                row.Col53 = line.Split("\t")[53].ToString();

                row.Col54 = line.Split("\t")[54].ToString();

                row.Col55 = line.Split("\t")[55].ToString();

                row.Col56 = line.Split("\t")[56].ToString();

                row.Col57 = line.Split("\t")[57].ToString();

                row.Col58 = line.Split("\t")[58].ToString();

                row.Col59 = line.Split("\t")[59].ToString();

                row.Col60 = line.Split("\t")[60].ToString();

                row.Col61 = line.Split("\t")[61].ToString();

                row.Col62 = line.Split("\t")[62].ToString();

                row.Col63 = line.Split("\t")[63].ToString();

                row.Col64 = line.Split("\t")[64].ToString();

                row.Col65 = line.Split("\t")[65].ToString();

                row.Col66 = line.Split("\t")[66].ToString();

                row.Col67 = line.Split("\t")[67].ToString();

                if (countHeader == 0)
                {
                    row.Col68 = vat;
                    row.Col69 = computedbaseamnt;
                    row.Col70 = computedbaseamnt1;
                }
                countHeader++;

                try
                {
                    double recamnt = Convert.ToDouble(row.Col51);

                    double totalchamnt = Convert.ToDouble(row.Col53);

                    double calcvat = (totalchamnt * per);

                    double computedbase1 = Convert.ToDouble(row.Col43);

                    if (row.Col59 != null && row.Col59.ToString() == "S")
                    {
                        row.Col68 = calcvat.ToString();
                    }
                    if (row.Col59 != null && row.Col59.ToString() == "S")
                    {
                        row.Col69 = Math.Round(recamnt + totalchamnt + calcvat, MidpointRounding.AwayFromZero).ToString().TrimStart().TrimEnd();
                    }
                    if (row.Col59 != null && row.Col59.ToString() == "S" && row.Col1 != null && row.Col1.ToString() == "A")
                    {
                        row.Col69 = Math.Round(recamnt + totalchamnt, MidpointRounding.AwayFromZero).ToString().TrimStart().TrimEnd();
                    }
                    if (row.Col59 != null && row.Col59.ToString() == "S")
                    {
                        row.Col70 = row.Col69;
                    }
                    if (row.Col59 != null && row.Col59.ToString() == "P")
                    {
                        row.Col70 = Math.Truncate(computedbase1).ToString().TrimStart().TrimEnd();
                    }
                    if (row.Col59 != null && row.Col59.ToString().Contains("P"))
                    {
                        row.Col69 = row.Col43.TrimStart().TrimEnd();
                    }
                }
                catch (Exception)
                {

                }

                list.Add(row);
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                var outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                var fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm}_WUSRW_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
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
    }
}