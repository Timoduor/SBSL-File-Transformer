using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using ExcelDataReader;
using SbslFileTransformer.Converters.Rwanda.BNR;

namespace SbslFileTransformer.Converters.Rwanda
{
    public class MoneyGramSettlementRWConverter
    {
        public MoneyGramSettlementRWConverter()
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
                    int countHeader = 0;
                    int count = 0;
                    double per = 0.18;
                    double ch = 0.78;
                    string accountNo = "Account Number";
                    string agentName = "Agent Name";
                    string vat = "VAT";
                    string mk = "MK";
                    string computedbaseamnt = "Computed Base Amount";
                    string charge = "Charge";
                    string revenue = "Revenue";
                    string amountFinal = "Amount Final";
                    string bankCode = "";
                    string agentDetails = "";
                    string tran = "";
                    string set = "";
                    string settelementCurrency = "Settlement Currency";
                    string transactionCurrency = "Transaction Currency";
                    while (reader.Read())
                    {
                        count++;
                        var row = new ExcelCols();
                        //settelement currency
                        //var value = reader.GetValue(1)?.ToString();
                        //transaction currency
                        var valueTran = reader.GetValue(9)?.ToString();
                        //settelement currency && account number
                        var accountValue = reader.GetValue(1)?.ToString();
                        //agentname
                        var agentValue = reader.GetValue(4)?.ToString();
                        //remember this code for dealing with empty rows
                        if (!string.IsNullOrEmpty(accountValue))
                        {
                            if (accountValue.Contains("Settlement Id") ||
                                accountValue.Contains("Business Date"))
                            {
                                continue;
                            }
                            else if (accountValue.Contains("Account Number"))
                            {
                                string rec = reader.GetValue(5)?.ToString();
                                bankCode = rec;
                            }
                            else if (accountValue.Contains("Settlement Currency"))
                            {
                                set = reader.GetValue(7)?.ToString();
                            }
                        }
                        if (agentValue != null)
                        {
                            if (agentValue.Contains("Agent Name"))
                            {
                                agentDetails = reader.GetValue(8)?.ToString();
                            }
                        }
                        if (valueTran != null)
                        {
                            if (valueTran.Contains("Transaction Currency"))
                            {
                                tran = reader.GetValue(12)?.ToString();
                            }
                        }
                        if (countHeader == 2)
                        {
                            row.Col0 = "Account Number";
                            row.Col1 = agentName;
                            row.Col16 = mk;
                            row.Col17 = vat;
                            row.Col19 = computedbaseamnt;
                            row.Col20 = charge;
                            row.Col21 = revenue;
                            row.Col22 = amountFinal;
                            row.Col23 = settelementCurrency;
                            row.Col24 = transactionCurrency;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(agentValue))
                            {
                                if (agentValue.Contains("Agent Name"))
                                {
                                    agentDetails = reader.GetValue(8)?.ToString();
                                }
                            }
                            row.Col0 = bankCode;
                            row.Col1 = agentDetails;
                            row.Col23 = set;
                            row.Col24 = tran;
                        }
                        //tran date
                        row.Col2 = reader.GetValue(1)?.ToString().Replace("\n", "");
                        //tran id
                        row.Col3 = reader.GetValue(3)?.ToString().Replace("\n", "");
                        //ref #
                        row.Col4 = reader.GetValue(7)?.ToString().Replace("\n", "");
                        //prod
                        row.Col5 = reader.GetValue(9)?.ToString().Replace("\n", "");
                        //tran type
                        row.Col6 = reader.GetValue(10)?.ToString().Replace("\n", "");
                        //origin cntry
                        row.Col7 = reader.GetValue(11)?.ToString().Replace("\n", "");
                        //rev cntry
                        row.Col8 = reader.GetValue(13)?.ToString().Replace("\n", "");
                        //fx rate
                        row.Col9 = reader.GetValue(15)?.ToString().Replace("\n", "");
                        //fx date
                        row.Col10 = reader.GetValue(19)?.ToString().Replace("\n", "");
                        //fx margin
                        row.Col11 = reader.GetValue(20)?.ToString().Replace("\n", "");
                        //base amount 
                        row.Col12 = reader.GetValue(23)?.ToString().Replace("\n", "");
                        //fee amount
                        row.Col13 = reader.GetValue(24)?.ToString().Replace("\n", "") + reader.GetValue(25)?.ToString().Replace("\n", "");
                        //fx rev share amount
                        row.Col14 = reader.GetValue(28)?.ToString().Replace("\n", "") + reader.GetValue(29)?.ToString().Replace("\n", "");
                        //commission amount
                        row.Col15 = reader.GetValue(32)?.ToString().Replace("\n", "") + reader.GetValue(33)?.ToString().Replace("\n", "");
                        //mk
                        if (reader.GetValue(34) != null)
                        {
                            row.Col16 = reader.GetValue(34)?.ToString().Replace("\n", "");
                        }
                        //total
                        row.Col18 = reader.GetValue(35)?.ToString().Replace("\n", "");
                        countHeader++;
                        try
                        {
                            double baseamnt = Convert.ToDouble(reader.GetValue(23));
                            double feeamnt = Convert.ToDouble(reader.GetValue(24));
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "SEN")
                            {
                                //VAT
                                row.Col17 = (feeamnt * per).ToString();
                                row.Col19 = Math.Ceiling(baseamnt + feeamnt + (feeamnt * per)).ToString();
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "RSN")
                            {
                                //VAT
                                row.Col17 = (feeamnt * per).ToString();
                                row.Col19 = Math.Ceiling(baseamnt + feeamnt + (feeamnt * per)).ToString();
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "REC")
                            {
                                row.Col19 = Math.Truncate(baseamnt).ToString();
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "SEN" && reader.GetValue(34) != null && reader.GetValue(34).ToString() == "mk")
                            {
                                row.Col19 = reader.GetValue(32)?.ToString().Replace("\n", "");
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "RSN" && reader.GetValue(34) != null && reader.GetValue(34).ToString() == "mk")
                            {
                                row.Col19 = reader.GetValue(32)?.ToString().Replace("\n", "");
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "REF")
                            {
                                row.Col19 = reader.GetValue(32)?.ToString().Replace("\n", "");
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "RDT")
                            {
                                row.Col19 = reader.GetValue(32)?.ToString().Replace("\n", "");
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "SEN")
                            {
                                //charge
                                row.Col20 = (feeamnt * ch).ToString();
                            }
                            if (reader.GetValue(10) != null && reader.GetValue(10).ToString() == "RSN")
                            {
                                //charge
                                row.Col20 = (feeamnt * ch).ToString();
                            }
                        }
                        catch (Exception)
                        {
                        }
                        if (!string.IsNullOrEmpty(row.Col2))
                        {
                            if (row.Col2.Contains("/") || row.Col2.Contains("Tran Date")
                            || row.Col2.Contains("RW") || row.Col2.Contains("KIGALI")
                            || row.Col2.Contains("3 AVENUE KN 9"))
                            {
                                list.Add(row);
                            }
                        }
                    }
                }
            }
            list[3].Col0 = list[2].Col0;
            list[3].Col1 = list[2].Col1;
            list[3].Col16 = list[2].Col16;
            list[3].Col17 = list[2].Col17;
            list[3].Col19 = list[2].Col19;
            list[3].Col20 = list[2].Col20;
            list[3].Col21 = list[2].Col21;
            list[3].Col22 = list[2].Col22;
            list[3].Col23 = list[2].Col23;
            list[3].Col24 = list[2].Col24;
            double rev1 = 0.4;
            double rev2 = 0.5;
            double amntfinal = 0.022;
            var zero = 0;
            var finalList = new List<ExcelCols>();
            foreach (var rows in list)
            {
                try
                {
                    if (rows.Col6 == null)
                    {
                        continue;
                    }
                    if (rows.Col6.Contains("SEN") && rows.Col1.Contains("COPEDU") || rows.Col1.Contains("GOSHEN"))
                    {
                        //revenue
                        //rows.Col21 = ((Convert.ToDouble(rows.Col13) - Convert.ToDouble(rows.Col20)) * rev2).ToString();
                        rows.Col21 = (Convert.ToDouble(rows.Col15) * rev2 * -1).ToString();
                    }
                    if (rows.Col6.Contains("SEN") && rows.Col1.Contains("EXTRA") || rows.Col1.Contains("RIM") || rows.Col1.Contains("AB BANK"))
                    {
                        //revenue
                        //rows.Col21 = ((Convert.ToDouble(rows.Col13) - Convert.ToDouble(rows.Col20)) * rev1).ToString();
                        rows.Col21 = (Convert.ToDouble(rows.Col15) * rev1 * -1).ToString();
                    }
                    if (!string.IsNullOrEmpty(rows.Col16))
                    {
                        if (rows.Col6.Contains("SEN") && rows.Col16.Contains("mk"))
                        {
                            rows.Col21 = Convert.ToDouble(zero).ToString();
                        }
                    }
                    if (!string.IsNullOrEmpty(rows.Col16))
                    {
                        if (rows.Col6.Contains("REC") && rows.Col16.Contains("mk"))
                        {
                            rows.Col21 = Convert.ToDouble(zero).ToString();
                        }
                    }
                    if (rows.Col6.Contains("SEN") || rows.Col1.Contains("COPEDU") || rows.Col1.Contains("GOSHEN") || rows.Col1.Contains("EXTRA") || rows.Col1.Contains("RIM") || rows.Col1.Contains("AB BANK"))
                    {
                        //amount final
                        rows.Col22 = (Convert.ToDouble(rows.Col12) + Convert.ToDouble(rows.Col20) + Convert.ToDouble(rows.Col21) + (Convert.ToDouble(rows.Col13) * amntfinal)).ToString();
                    }
                    if (rows.Col6.Contains("RSN") || rows.Col1.Contains("COPEDU") || rows.Col1.Contains("GOSHEN") || rows.Col1.Contains("EXTRA") || rows.Col1.Contains("RIM") || rows.Col1.Contains("AB BANK"))
                    {
                        //amount final
                        rows.Col22 = (Convert.ToDouble(rows.Col12) + Convert.ToDouble(rows.Col20) + Convert.ToDouble(rows.Col21) + (Convert.ToDouble(rows.Col13) * amntfinal)).ToString();
                    }
                    if (rows.Col6.Contains("REC") || rows.Col6.Contains("RDT"))
                    {
                        //amount final
                        rows.Col22 = (Convert.ToDouble(rows.Col12)).ToString();
                    }
                    finalList.Add(rows);
                }
                catch (Exception)
                {
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                string outputFolder = Path.Combine(Path.GetDirectoryName(inputFile), "Conv");
                Directory.CreateDirectory(outputFolder);

                string fileName = Path.GetFileNameWithoutExtension(inputFile);

                outputFile = Path.Combine(outputFolder,
                    $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}_MG_{fileName.Substring(Math.Max(0, fileName.Length - 14)).Replace(" ", "")}.csv");
            }

            this.WriteToFile(finalList, outputFile);
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