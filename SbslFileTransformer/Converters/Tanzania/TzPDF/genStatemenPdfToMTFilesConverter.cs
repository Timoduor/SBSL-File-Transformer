using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace SbslFileTransformer.Converters.Tanzania.TzPDF
{
    public class genStatemenPdfToMTFilesConverter
    {
       
        public void ConvertFile_Tiss(string inputFile, string password = "", string outputFile = null)
        {

            outputFile = System.IO.Path.GetDirectoryName(inputFile) + "\\conv\\MT940_" + System.IO.Path.GetFileNameWithoutExtension(inputFile) + ".txt";

            string text = GetTextFromPdf(inputFile, password);

            string bankAcc = string.Empty;
            string currency = string.Empty;
            List<ExtractedTableCRDB> transactions = new List<ExtractedTableCRDB>();
            bool isNewTableLine = true;
            double closingBal = 0;

            ExtractedTableCRDB extractedTableLine = null;

            bool areTableValues = false;


            string statementno = "";
            string businessDate = "";
            double openingBal = 0;
            string Total_Debits = "";
            string Total_Credits = "";
            string Total_Debit = "";
            string Total_Credit = "";

            try
            {
                string[] Itmes_ = text.Split('\n', '\r');
                for (int i = 1; i < Itmes_.Length; i++)
                {
                    if (Itmes_[i].Contains("Account No."))
                    {
                        bankAcc = Itmes_[i].Split('.')[1].Trim();
                        continue;
                    }
                    if (Itmes_[i].Contains("Statement no"))
                    {
                        statementno = Itmes_[i].Split(' ')[2];
                        continue;
                    }
                    if (Itmes_[i].Contains("Business Date"))
                    {
                        businessDate = Itmes_[i].Split(' ')[2].Replace('-', ' ').Replace(" ", "").Substring(2, 6);
                        continue;
                    }
                    if (Itmes_[i].Contains("Statement Report for"))
                    {
                        currency = Itmes_[i].Split(' ')[3].Trim().ToUpper();
                        continue;
                    }
                    if (Itmes_[i].Contains("Closing balance"))
                    {
                        closingBal = Convert.ToDouble(Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1]);
                    }
                    if (Itmes_[i].Contains("Opening balance"))
                    {
                        openingBal = Convert.ToDouble(Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1]);
                    }
                    if (Itmes_[i].Contains("Total Debits"))
                    {
                        Total_Debits = Itmes_[i].Split(' ')[2];
                    }
                    if (Itmes_[i].Contains("Total Credits"))
                    {
                        Total_Credits = Itmes_[i].Split(' ')[2];
                    }

                    if (Itmes_[i].Contains("Total Debit") && !Itmes_[i].Contains("Total Debits"))
                    {
                        Total_Debit = Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1];
                    }
                    if (Itmes_[i].Contains("Total Credit") && !Itmes_[i].Contains("Total Credits"))
                    {
                        Total_Credit = Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1];
                    }
                }


            }
            catch (Exception ex)
            {

            }
            Boolean got_amt = false;
            Boolean gotref = false;
            Boolean gotdate = false;
            string xline = "";
            string xrecs = "";
            string Xref = "";
            string XRemark = "";

            foreach (string line in text.Split('\n', '\r'))
            {
                if (line.Contains("Additional Information"))
                {
                    areTableValues = true;
                    continue;
                }

                if (areTableValues)
                {
                    string[] columns = line.Split(" ");// line.Split('\t'); // Assuming tab-separated values

                    for (int i = 0; i < columns.Length; i++)
                    {
                        string column = columns[i];

                        //xline = xline + " " + columns[i];
                        if (Regex.IsMatch(column, @"\d+(\.\d+)?,\s*"))
                        {
                            
                            if (column.Replace(",", "").Replace(".", ",").Split(',').Length == 2)
                            {
                                if (column != "")
                                {
                                    if (Regex.Replace(column.Replace(",", "").Replace(".", ",").Split(',')[0], "[^0-9]", "") != "")
                                    {
                                        column = Regex.Replace(column.Replace(",", "").Replace(".", ",").Split(',')[0], "[^0-9]", "") + "," + column.Replace(",", "").Replace(".", ",").Split(',')[1];
                                        //column = column.Replace(",", "").Replace(".", ",").Split(',')[0] + "," + column.Replace(",", "").Replace(".", ",").Split(',')[1];
                                        /*   Regex.Replace(column, "[^0-9]", "")*/
                                        ;
                                        if (XRemark.Length > 50)
                                        {
                                            xline = ":61:" + businessDate + "C" + column + "S103 " + RemoveSpecialCharacters(Xref);
                                        }
                                        else
                                        {
                                            xline = ":61:" + businessDate + "C" + column + "S103 " + RemoveSpecialCharacters(Xref);
                                        }

                                        got_amt = true;
                                        gotref = false;
                                        xline = xline + Environment.NewLine;
                                        Xref = "";
                                        XRemark = "";
                                        xrecs = xrecs + xline;
                                    
                                    }

                                }

                            }

                     
                        }
                        else
                        {
                            if (gotref == false)
                            {
                                Xref = column;
                                gotref = true;
                                continue;
                            }
                            if (gotdate == false || column.Length == 8)
                            {
                                gotdate = true;
                                continue;
                            }

                            XRemark = XRemark + " " + column;

                        }


                    }
                     
                }
            }

            StringBuilder lines = new StringBuilder();
 

            lines.AppendLine(":20:" + "1");
            lines.AppendLine(":25:" + bankAcc);
            lines.AppendLine(":28C:" + "1/1");
            lines.AppendLine(":60M:" + $@"C{businessDate:yyMMdd}{currency}0,00");

            foreach (ExtractedTableCRDB record in transactions)
            {
                DateTime valDate =
                    DateTime.ParseExact(record.ValueDate, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                string valDateStr = valDate.ToString("yyMMdd");
                string valDateStr2 = valDate.ToString("MMdd");

                string dOrC = "C";

                double amountC = Convert.ToDouble(record.Credit);
                double amountD = Convert.ToDouble(record.Debit);

                bool useC = true;
                if (amountC > 0)
                {
                    dOrC = "C";
                }
                else if (amountD > 0)
                {
                    useC = false;
                    dOrC = "D";
                }

                string narrative = $"{record.Ref?.Trim()}";
                string c61 =
                    $"{valDateStr}{valDateStr2}{dOrC}R{(useC ? amountC.ToString("N2").Replace(",", "").Replace(".", ",") : amountD.ToString("N2").Replace(",", "").Replace(".", ","))}S205{narrative}";

                lines.AppendLine($":61:{c61}  {record.Details?.Trim()}");
            }
            lines.AppendLine(xrecs);

            lines.AppendLine(":62F:" +
                             $@"C{businessDate:yyMMdd}{currency}{closingBal.ToString("N2").Replace(",", "").Replace(".", ",")}");

            string fileName = Path.GetFileNameWithoutExtension(inputFile);

            File.WriteAllText(outputFile, lines.ToString());
        }



        static string RemoveSpecialCharacters(string input)
        {
            char[] specialCharacters = "!@#$%^&*()_+[]{}|;:'<>,.?/~`".ToCharArray();

            // Loop through each character in the input string
            for (int i = 0; i < specialCharacters.Length; i++)
            {
                input = input.Replace(specialCharacters[i].ToString(), "");
            }

            return input;
        }
        public class ExtractedTableCRDB
        {
            public string PostingDate { get; set; }
            public string Details { get; set; }
            public string Ref { get; set; }
            public string ValueDate { get; set; }
            public string Debit { get; set; }
            public string Credit { get; set; }
            public string BookBalance { get; set; }
        }
        public void ConvertFile_crd(string inputFile, string password = "", string outputFile = null)
        {
            string folderPath = System.IO.Path.GetDirectoryName(inputFile);
            string csv_file = "";   
            string bankAcc = "";
            string statementno = "";
            string businessDate = "";
            double closingBal = 0;
            double openingBal = 0;
            string currency = string.Empty;
            string Total_Debits = "";
            string Total_Credits = "";
            string Total_Debit = "";
            string Total_Credit = "";

            csv_file = System.IO.Path.ChangeExtension(inputFile, null) + ".csv";
            outputFile = System.IO.Path.GetFileNameWithoutExtension(inputFile);

            if (System.IO.File.Exists(csv_file)) 
            {
                if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(inputFile) + "\\conv\\"))
                {
                }
                else
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(inputFile) + "\\conv\\");
                }

                string text = GetTextFromPdf(inputFile, "");
                outputFile = System.IO.Path.GetDirectoryName(inputFile) + "\\conv\\" + "MT940_" + System.IO.Path.GetFileNameWithoutExtension(inputFile).Replace(" ", "") + ".txt";


                try
                {
                    string[] Itmes_ = text.Split('\n', '\r');
                    for (int i = 1; i < Itmes_.Length; i++)
                    {
                        if (Itmes_[i].Contains("Account No."))
                        {
                            bankAcc = Itmes_[i].Split('.')[1].Trim();
                            continue;
                        }
                        if (Itmes_[i].Contains("Statement no"))
                        {
                            statementno = Itmes_[i].Split(' ')[2];
                            continue;
                        }
                        if (Itmes_[i].Contains("Business Date"))
                        {
                            businessDate = Itmes_[i].Split(' ')[2].Replace('-', ' ').Replace(" ", "").Substring(2, 6);
                            continue;
                        }
                        if (Itmes_[i].Contains("Statement Report for"))
                        {
                            currency = Itmes_[i].Split(' ')[3].Trim().ToUpper();
                            continue;
                        }
                        if (Itmes_[i].Contains("Closing balance"))
                        {
                            closingBal = Convert.ToDouble(Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1]);
                        }
                        if (Itmes_[i].Contains("Opening balance"))
                        {
                            openingBal = Convert.ToDouble(Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1]);
                        }
                        if (Itmes_[i].Contains("Total Debits"))
                        {
                            Total_Debits = Itmes_[i].Split(' ')[2];
                        }
                        if (Itmes_[i].Contains("Total Credits"))
                        {
                            Total_Credits = Itmes_[i].Split(' ')[2];
                        }

                        if (Itmes_[i].Contains("Total Debit") && !Itmes_[i].Contains("Total Debits"))
                        {
                            Total_Debit = Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1];
                        }
                        if (Itmes_[i].Contains("Total Credit") && !Itmes_[i].Contains("Total Credits"))
                        {
                            Total_Credit = Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "." + Itmes_[i].Split(' ')[2].Trim().Replace(',', ' ').Trim().Split('.')[1];
                        }
                    }


                }
                catch (Exception ex)
                {

                }

                try
                {

                    string content = File.ReadAllText(csv_file);
                    if (currency == "USD")
                    {
                        Create_USD_MT940(content, statementno, bankAcc, currency, businessDate, closingBal, openingBal, outputFile);


                    }
                    else if (currency == "TZS")
                    {
                        Create_TZS_MT940(content, statementno, bankAcc, currency, businessDate, closingBal, openingBal, outputFile);
                    }
                }
                catch (Exception ex)
                {

                }
            }

        }



        private void Create_TZS_MT940(string content, string statementno, string bankAcc, string currency, string businessDate, double closingBal, double openingBal, string outputFile)
        {

            string fld20 = "";
            string fld25 = "";
            string fld28C = "";
            string fld28C_ = "";
            string fld60M = "";
            string fld61 = "";
            string fld61_ = "";
            string fld86 = "";
            string fld86_ = "";
            string fld62F = "";
            string addlIn = "";
            fld20 = ":20:" + statementno + "/" + bankAcc;
            fld25 = ":25:" + bankAcc;
            fld28C = ":28C:" + statementno.PadLeft(4, '0') + "/00001";
            if (closingBal > 0)
            {

                fld62F = "62F" + ("C" + businessDate + currency.ToUpper() + closingBal).Replace('.', ',');
            }
            else
            {

                fld62F = "62F" + ("D" + businessDate + currency.ToUpper() + closingBal).Replace('.', ',');
            }
            if (openingBal > 0)
            {
                fld60M = ":60F:" + ("C" + businessDate + currency.ToUpper() + openingBal).Replace('.', ',');
            }
            else
            {

                fld60M = ":60F:" + ("D" + businessDate + currency.ToUpper() + openingBal).Replace('.', ',');
            }
            var tm = DateTime.Now.ToString("HHmm");

            var s = "{1:F01IMBLTZTZAXXX" + DateTime.Now.ToString("HHmm") + "}{2:O940" + tm + businessDate + "IMBLTZTZA" + "XXXX" + bankAcc + businessDate + tm + "N}{4:" + Environment.NewLine;
            s += fld20 + Environment.NewLine;
            s += fld25 + Environment.NewLine;
            s += fld28C + Environment.NewLine;
            s += fld60M + Environment.NewLine;

            string[] sGrp = content.Split('\n');



            if (sGrp.Length != 0)
            {
                for (int i = 1; i < sGrp.Length; i++)
                {
                    if (sGrp[i] != "")
                    {
                        if (sGrp[i].Split('|')[3] != "")
                        {
                            if (sGrp[i].Split('|')[3].Trim().Split(' ')[1] == "001FTOL230860259")
                            {

                            }
                        }
                    }
                    string[] splitValues = sGrp[i].Split('|');
                    if (splitValues.Length > 1)
                    {
                        string secondValue = splitValues[1];
                        if (int.TryParse(secondValue, out int intValue) || double.TryParse(secondValue, out double doubleValue))
                        {

                            if (fld61_ != "" && fld61_.Length < 35)
                            {

                                fld61 += fld61_ + " " + addlIn + Environment.NewLine;
                                fld86 += fld86_;  //
                                s += fld61_ + " " + addlIn + Environment.NewLine + fld86_;
                                fld61_ = "";
                                addlIn = "";
                                fld86_ = "";
                                if (sGrp[i].Split('|')[6] != null)
                                {


                                    if (sGrp[i].Split('|').Length > 14)
                                    {
                                        if (sGrp[i].Split('|')[8] != "")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            fld28C_ = statementno + "/" + sGrp[i].Split('|')[6].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + "D" + sGrp[i].Split('|')[8].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[8].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                        }
                                        else if (sGrp[i].Split('|')[11] != "")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            fld28C_ = statementno + "/" + sGrp[i].Split('|')[6].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + "D" + sGrp[i].Split('|')[11].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[11].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                        }
                                        else if (sGrp[i].Split('|')[12] != "" && sGrp[i].Split('|')[7] != " ")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            fld28C_ = statementno + "/" + sGrp[i].Split('|')[6].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + "D" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                        }
                                        else if (sGrp[i].Split('|')[14] != "" || sGrp[i].Split('|')[7] != "")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            fld28C_ = statementno + "/" + sGrp[i].Split('|')[6].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                        }
                                        else if (sGrp[i].Split('|')[14] != "")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                        }
                                    }
                                    else if (sGrp[i].Split('|')[14] != "")
                                    {
                                        fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                        addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                        fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                    }
                                    else if (sGrp[i].Split('|')[10] != "")
                                    {
                                        fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                        addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                        fld61_ += ":61:" + businessDate + "D" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                    }
                                    else
                                    {
                                        fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                        addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                        fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                    }

                                }
                            }
                            else if (fld61_ == " " && addlIn == " ")
                            {
                                if (i != 1)
                                {
                                    fld61 = fld61_ + " " + addlIn;
                                    s += fld61_ + " " + addlIn + Environment.NewLine + fld86;
                                    fld61_ = "";
                                    addlIn = "";
                                }

                                if (sGrp[i].Split('|')[10] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    if (sGrp[i].Split('|')[12] != "")
                                    {
                                        fld61_ += ":61:" + businessDate + "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                    }
                                    else
                                    {
                                        fld61_ += ":61:" + businessDate + "C" + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                    }

                                }
                            }
                            else
                            {
                                if (sGrp[i].Split('|')[12] != "" || sGrp[i].Split('|')[7] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                                else if (sGrp[i].Split('|')[15] != "" && sGrp[i].Split('|')[7] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                                else if (sGrp[i].Split('|')[14] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                                else
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                            }


                        }
                        else
                        {
                            if (sGrp[i].Split('|')[6] != null)
                            {

                                fld86_ += sGrp[i].Split('|')[6] != "" ? sGrp[i].Split('|')[6].Trim() + Environment.NewLine : sGrp[i].Split('|')[10].Trim() + Environment.NewLine;

                            }

                        }
                    }



                }

            }
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                fld61 += fld61_ + " " + addlIn + Environment.NewLine;
                fld86 += fld86_; ;
                s += fld61_ + " " + addlIn + Environment.NewLine + fld86_;
                fld61_ = "";
                addlIn = "";
                fld86_ = "";

                s += fld62F + Environment.NewLine;
                s += " -}" + Environment.NewLine;
                writer.WriteLine(s);
            }
        }
        private void Create_USD_MT940(string content, string statementno, string bankAcc, string currency, string businessDate, double closingBal, double openingBal, string outputFile)
        {
            string fld20 = "";
            string fld25 = "";
            string fld28C = "";
            string fld28C_ = "";
            string fld60M = "";
            string fld61 = "";
            string fld61_ = "";
            string fld86 = "";
            string fld86_ = "";
            string fld62F = "";
            string addlIn = "";
            fld20 = ":20:" + statementno + "/" + bankAcc;
            fld25 = ":25:" + bankAcc;
            fld28C = ":28C:" + statementno.PadLeft(4, '0') + "/00001";
            if (closingBal > 0)
            {

                fld62F = "62F" + ("C" + businessDate + currency.ToUpper() + closingBal).Replace('.', ',');
            }
            else
            {

                fld62F = "62F" + ("D" + businessDate + currency.ToUpper() + closingBal).Replace('.', ',');
            }
            if (openingBal > 0)
            {
                fld60M = ":60F:" + ("C" + businessDate + currency.ToUpper() + openingBal).Replace('.', ',');
            }
            else
            {

                fld60M = ":60F:" + ("D" + businessDate + currency.ToUpper() + openingBal).Replace('.', ',');
            }
            var tm = DateTime.Now.ToString("HHmm");

            var s = "{1:F01IMBLTZTZAXXX" + DateTime.Now.ToString("HHmm") + "}{2:O940" + tm + businessDate + "IMBLTZTZA" + "XXXX" + bankAcc + businessDate + tm + "N}{4:" + Environment.NewLine;
            s += fld20 + Environment.NewLine;
            s += fld25 + Environment.NewLine;
            s += fld28C + Environment.NewLine;
            s += fld60M + Environment.NewLine;

            string[] sGrp = content.Split('\n');


            if (sGrp.Length != 0)
            {
                for (int i = 1; i < sGrp.Length; i++)
                {

                    string[] splitValues = sGrp[i].Split('|');
                    if (splitValues.Length > 1)
                    {
                        string secondValue = splitValues[1];
                        if (int.TryParse(secondValue, out int intValue) || double.TryParse(secondValue, out double doubleValue))
                        {

                            if (fld61_ != "" && fld61_.Length < 35)
                            {

                                fld61 += fld61_ + " " + addlIn + Environment.NewLine;
                                fld86 += fld86_;  //
                                s += fld61_ + " " + addlIn + Environment.NewLine + fld86_;
                                fld61_ = "";
                                addlIn = "";
                                fld86_ = "";
                                if (sGrp[i].Split('|')[6] != null)
                                {

                                    if (sGrp[i].Split('|').Length > 14)
                                    {
                                        if (sGrp[i].Split('|')[12] != "" && sGrp[i].Split('|')[7] != " ")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            fld28C_ = statementno + "/" + sGrp[i].Split('|')[6].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + "D" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                        }
                                        else if (sGrp[i].Split('|')[15] != "" && sGrp[i].Split('|')[7] != "")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            fld28C_ = statementno + "/" + sGrp[i].Split('|')[6].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                        }
                                        else if (sGrp[i].Split('|')[14] != "")
                                        {
                                            fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                            addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                            fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                        }
                                    }
                                    else if (sGrp[i].Split('|')[14] != "")
                                    {
                                        fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                        addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                        fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                    }
                                    else if (sGrp[i].Split('|')[10] != "")
                                    {
                                        fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                        addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                        fld61_ += ":61:" + businessDate + "D" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                    }
                                    else
                                    {
                                        fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                        addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                        fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                    }

                                }
                            }
                            else if (fld61_ == " " && addlIn == " ")
                            {
                                if (i != 1)
                                {
                                    fld61 = fld61_ + " " + addlIn;
                                    s += fld61_ + " " + addlIn + Environment.NewLine + fld86;
                                    fld61_ = "";
                                    addlIn = "";
                                }

                                if (sGrp[i].Split('|')[10] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    if (sGrp[i].Split('|')[12] != "")
                                    {
                                        fld61_ += ":61:" + businessDate + "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                    }
                                    else
                                    {
                                        fld61_ += ":61:" + businessDate + "C" + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "");
                                    }

                                }
                            }
                            else
                            {
                                if (sGrp[i].Split('|')[15] != "" && sGrp[i].Split('|')[7] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[15].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                                else if (sGrp[i].Split('|')[14] != "")
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[14].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                                else
                                {
                                    fld86_ = ":86:" + sGrp[i].Split('|')[6] != "" ? ":86:" + sGrp[i].Split('|')[6].Trim() : ":86:" + sGrp[i].Split('|')[10].Trim();
                                    addlIn = "S" + sGrp[i].Split('|')[3].Trim().Split(' ')[0] + sGrp[i].Split('|')[3].Trim().Split(' ')[1];
                                    fld61_ += ":61:" + businessDate + (sGrp[i].Split('|')[7] != "" ? "D" + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[7].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", "") : "C" + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[0].Replace(" ", "") + "," + sGrp[i].Split('|')[12].Replace('\r', ' ').Replace(',', ' ').Trim().Split('.')[1].Replace(" ", ""));
                                }
                            }


                        }
                        else
                        {
                            if (sGrp[i].Split('|')[6] != null)
                            {

                                fld86_ += sGrp[i].Split('|')[6] != "" ? sGrp[i].Split('|')[6].Trim() + Environment.NewLine : sGrp[i].Split('|')[10].Trim() + Environment.NewLine;

                            }

                        }
                    }



                }

            }
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                fld61 += fld61_ + " " + addlIn + Environment.NewLine;
                fld86 += fld86_; ;
                s += fld61_ + " " + addlIn + Environment.NewLine + fld86_;
                fld61_ = "";
                addlIn = "";
                fld86_ = "";

                s += fld62F + Environment.NewLine;
                s += " -}" + Environment.NewLine;
                writer.WriteLine(s);
            }
        }


        public static string GetTextFromPdf(string path, string password = "")
        {
            StringBuilder content = new StringBuilder();

         //   ReaderProperties readProps = new ReaderProperties().SetPassword(Encoding.Default.GetBytes(password));

            using (PdfReader reader = new PdfReader(path))
            {
                PdfDocument pdfDocument = new PdfDocument(reader);

                int pages = pdfDocument.GetNumberOfPages();

                for (int i = 1; i <= pages; i++)
                {
                    SimpleTextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    PdfPage page = pdfDocument.GetPage(i);

                    string text = PdfTextExtractor.GetTextFromPage(page, strategy);

                    content.Append(text);
                }
            }

            return content.ToString();
        }
        public class ExtractedTableCRDB1
        {
            public string PostingDate { get; set; }
            public string Details { get; set; }
            public string Ref { get; set; }
            public string ValueDate { get; set; }
            public string Debit { get; set; }
            public string Credit { get; set; }
            public string BookBalance { get; set; }
        }
        
    }
}
