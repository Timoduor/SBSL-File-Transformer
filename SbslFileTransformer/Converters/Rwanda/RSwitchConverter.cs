using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class RSwitchConverter
    {
        public RSwitchConverter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public void ConvertFile(string inputFile, string outputFile = null)
        {
            List<ExcelCols> list = new List<ExcelCols>();
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

            int countHeader = 0;

            string time = "Time";

            string transRef = "Trans Ref";

            string deviceID = "Device ID";

            string issuer = "Issuer";

            string pan = "PAN";

            string transactiont = "Transactiont";

            string msgType = "Msg Type";

            string respCode = "Resp Code";

            string response = "Response";

            string fee = "Fee";

            string valueAmnt = "Value";

            string[] lines = File.ReadAllLines(inputFile);

            foreach (string line in lines)
            {
                ExcelCols row = new ExcelCols();

                if (countHeader == 0)
                {
                    ExcelCols header = new ExcelCols();

                    header.Col0 = time;

                    header.Col1 = transRef;

                    header.Col2 = deviceID;

                    header.Col3 = issuer;

                    header.Col4 = pan;

                    header.Col5 = transactiont;

                    header.Col6 = msgType;

                    header.Col7 = respCode;

                    header.Col8 = response;

                    header.Col9 = fee;

                    header.Col10 = valueAmnt;

                    list.Add(header);
                }
                countHeader++;

                try
                {
                    string value = row.Col0;

                    row.Col0 = line.Split("\t")[0].ToString().Trim();

                    row.Col1 = line.Split("\t")[1].ToString().Trim();

                    row.Col2 = line.Split("\t")[2].ToString().Trim();

                    row.Col3 = line.Split("\t")[4].ToString().Trim();

                    row.Col4 = line.Split("\t")[6].ToString().Trim();

                    row.Col5 = line.Split("\t")[8].ToString() + line.Split("\t")[9].ToString().Trim();

                    row.Col6 = line.Split("\t")[12].ToString() + line.Split("\t")[14].ToString().Trim();

                    row.Col7 = line.Split("\t")[15].ToString() + line.Split("\t")[16].ToString().Trim();

                    row.Col8 = line.Split("\t")[18].ToString() + line.Split("\t")[19].ToString().Trim();

                    row.Col9 = line.Split("\t")[20].ToString() + line.Split("\t")[21].ToString().Trim();

                    row.Col10 = line.Split("\t")[22].ToString().Trim();

                    row.Col11 = line.Split("\t")[25].ToString().Trim();
                }
                catch (Exception)
                {

                }
                if (line.Contains("Time"))
                {
                    continue;
                }
                if (!line.Contains("CashW") && line.Contains("BalInq	"))
                {
                    continue;
                }
                if (row.Col5 == null)
                {
                    continue;
                }

                list.Add(row);
            }


            outputFile = outputFolder + "\\Converted_" + Path.GetFileNameWithoutExtension(inputFile) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".csv";
            this.WriteToFile(list, outputFile);
        }

        private void WriteToFile(List<ExcelCols> rows, string outputFile)
        {
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (ExcelCols row in rows)
                    {
                        csv.WriteRecord(row);
                        csv.NextRecord();
                    }
                }
            }
        }
    }
}
