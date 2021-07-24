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

            var countHeader = 0;

            var per = 0.18;

            var vat = "VAT";

            var computedbaseamnt = "Computed Base Amount";

            var computedbaseamnt1 = "Computed Base Amount without decimal";

            var lines = File.ReadAllLines(inputFile);

            foreach (var line in lines)
            {
                var row = new ExcelCols();

                row.Col0 = line.Split("\t")[0];

                row.Col1 = line.Split("\t")[1];

                row.Col2 = line.Split("\t")[2];

                row.Col3 = line.Split("\t")[3];

                row.Col4 = line.Split("\t")[4];

                row.Col5 = line.Split("\t")[5];

                row.Col6 = line.Split("\t")[6];

                row.Col7 = line.Split("\t")[7];

                row.Col8 = line.Split("\t")[8];

                row.Col9 = line.Split("\t")[9];

                row.Col10 = line.Split("\t")[10];

                row.Col11 = line.Split("\t")[11];

                row.Col12 = line.Split("\t")[12];

                row.Col13 = line.Split("\t")[13];

                row.Col14 = line.Split("\t")[14];

                row.Col15 = line.Split("\t")[15];

                row.Col16 = line.Split("\t")[16];

                row.Col17 = line.Split("\t")[17];

                row.Col18 = line.Split("\t")[18];

                row.Col19 = line.Split("\t")[19];

                row.Col20 = line.Split("\t")[20];

                row.Col21 = line.Split("\t")[21];

                row.Col22 = line.Split("\t")[22];

                row.Col23 = line.Split("\t")[23];

                row.Col24 = line.Split("\t")[24];

                row.Col25 = line.Split("\t")[25];

                row.Col26 = line.Split("\t")[26];

                row.Col27 = line.Split("\t")[27];

                row.Col28 = line.Split("\t")[28];

                row.Col29 = line.Split("\t")[29];

                row.Col30 = line.Split("\t")[30];

                row.Col31 = line.Split("\t")[31];

                row.Col32 = line.Split("\t")[32];

                row.Col33 = line.Split("\t")[33];

                row.Col34 = line.Split("\t")[34];

                row.Col35 = line.Split("\t")[35];

                row.Col36 = line.Split("\t")[36];

                row.Col37 = line.Split("\t")[37];

                row.Col38 = line.Split("\t")[38];

                row.Col39 = line.Split("\t")[39];

                row.Col40 = line.Split("\t")[40];

                row.Col41 = line.Split("\t")[41];

                row.Col42 = line.Split("\t")[42];

                row.Col43 = line.Split("\t")[43];

                row.Col44 = line.Split("\t")[44];

                row.Col45 = line.Split("\t")[45];

                row.Col46 = line.Split("\t")[46];

                row.Col47 = line.Split("\t")[47];

                row.Col48 = line.Split("\t")[48];

                row.Col49 = line.Split("\t")[49];

                row.Col50 = line.Split("\t")[50];

                row.Col51 = line.Split("\t")[51];

                row.Col52 = line.Split("\t")[52];

                row.Col53 = line.Split("\t")[53];

                row.Col54 = line.Split("\t")[54];

                row.Col55 = line.Split("\t")[55];

                row.Col56 = line.Split("\t")[56];

                row.Col57 = line.Split("\t")[57];

                row.Col58 = line.Split("\t")[58];

                row.Col59 = line.Split("\t")[59];

                row.Col60 = line.Split("\t")[60];

                row.Col61 = line.Split("\t")[61];

                row.Col62 = line.Split("\t")[62];

                row.Col63 = line.Split("\t")[63];

                row.Col64 = line.Split("\t")[64];

                row.Col65 = line.Split("\t")[65];

                row.Col66 = line.Split("\t")[66];

                row.Col67 = line.Split("\t")[67];

                if (countHeader == 0)
                {
                    row.Col68 = vat;
                    row.Col69 = computedbaseamnt;
                    row.Col70 = computedbaseamnt1;
                }

                countHeader++;

                try
                {
                    var recamnt = Convert.ToDouble(row.Col51);

                    var totalchamnt = Convert.ToDouble(row.Col53);

                    var calcvat = totalchamnt * per;

                    var computedbase1 = Convert.ToDouble(row.Col43);

                    if (row.Col59 != null && row.Col59 == "S") row.Col68 = calcvat.ToString();
                    if (row.Col59 != null && row.Col59 == "S")
                        row.Col69 = Math.Round(recamnt + totalchamnt + calcvat, MidpointRounding.AwayFromZero)
                            .ToString().TrimStart().TrimEnd();
                    if (row.Col59 != null && row.Col59 == "S") row.Col70 = row.Col69;
                    if (row.Col59 != null && row.Col59 == "P")
                        row.Col70 = Math.Truncate(computedbase1).ToString().TrimStart().TrimEnd();
                    if (row.Col59 != null && row.Col59.Contains("P")) row.Col69 = row.Col43.TrimStart().TrimEnd();
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