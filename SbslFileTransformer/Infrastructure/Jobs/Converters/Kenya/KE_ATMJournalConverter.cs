using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters.Kenya
{
    public class KE_ATMJournalConverter
    {


        public void ConvertFile_WinkaATMjrn(string inputFile)
        {

            string ATMNO = "";
            string outputFolder = null;

            string outputFile = "";

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
            }

            outputFolder = Path.GetFullPath(Path.Combine(outputFolder, @"..\")) + "Conv";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string[] sDet = File.ReadAllLines(inputFile);
            try
            {
                if (sDet[0].Length != 0)
                {
                    ATMNO = sDet[0].Split(" ")[0].Trim();
                }
            }
            catch (Exception)
            { }
            string content = File.ReadAllText(inputFile);


            string[] sGrp = content.Split("TRANSACTION START");
            string[] sGrp_s = content.Split("SUPERVISOR MODE ENTRY");
            string scontent = "";
            string scontent_ = "";
            string scontent_sup = "";
            ATMJournal ATMflds = new ATMJournal();

            bool hascashcount = false;

            try
            {
                if (sGrp.Length != 0)
                {

                    for (int i = 1; i < sGrp.Length; i++)
                    {

                        List<string> lx = this.GetJournalDetails(sGrp[i].Split("\n"), ATMNO);

                        if (lx.Count > 0)
                        {
                            ATMflds.CARDNo = lx.Any(p => p.StartsWith("CARDNO:")) ? lx.First(p => p.StartsWith("CARDNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.trnDATE = lx.Any(p => p.StartsWith("DATE:")) ? lx.First(p => p.StartsWith("DATE:")).Replace("DATE:", "") : "";
                            ATMflds.AMOUNT = lx.Any(p => p.StartsWith("AMOUNT:")) ? lx.First(p => p.StartsWith("AMOUNT:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                            ATMflds.UTRNNO = lx.Any(p => p.StartsWith("SEQ:")) ? lx.First(p => p.StartsWith("SEQ:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.ReasonCode = lx.Any(p => p.StartsWith("RESPONSE CODE:")) ? lx.First(p => p.StartsWith("RESPONSE CODE:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.AtmNo = lx.Any(p => p.StartsWith("ATMNO:")) ? lx.First(p => p.StartsWith("ATMNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.AUTHNO = lx.Any(p => p.StartsWith("AUTHNO:")) ? lx.First(p => p.StartsWith("AUTHNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.CURRENCY = lx.Any(p => p.StartsWith("CCY:")) ? lx.First(p => p.StartsWith("CCY:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.TRANSACTION_END = lx.Any(p => p.StartsWith("TRANSACTION_END|")) ? lx.First(p => p.StartsWith("TRANSACTION_END|")).Split('|')[1].ToString().Replace("|", "") : "";
                            ATMflds.TRANSACTION_START = lx.Any(p => p.StartsWith("TRANSACTION_START|")) ? lx.First(p => p.StartsWith("TRANSACTION_START|")).Split('|')[1].ToString().Replace("|", "") : "";
                            ATMflds.CASHTAKENZ = lx.Any(p => p.StartsWith("CASH TAKENZ|")) ? lx.First(p => p.StartsWith("CASH TAKENZ|")).Split('|')[1].ToString().Replace("|", "") : "";
                            ATMflds.REF2 = lx.Any(p => p.StartsWith("REFERENCE:")) ? lx.First(p => p.StartsWith("REFERENCE:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.Cashtaken = lx.Any(p => p.StartsWith("CASH TAKEN")) ? true : false;

                            if (ATMflds.Cashtaken == true)
                            {
                                ATMflds.SUCCESSFUL = "APPROVED";
                            }
                            else
                            {
                                ATMflds.SUCCESSFUL = "DECLINED";
                            }
                            if (ATMflds.CARDNo != "" && ATMflds.AMOUNT != "0")
                            {
                                ATMNO = ATMflds.AtmNo;
                                if (scontent_sup != "")
                                {

                                    if (scontent == "")
                                    {

                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + "," + ATMflds.TRANSACTION_START.Trim() + "," + ATMflds.TRANSACTION_END.Trim() + "," + ATMflds.REF2 + "," + ATMflds.CASHTAKENZ +  Environment.NewLine;

                                    }
                                    else
                                    {
                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + "," + ATMflds.TRANSACTION_START.Trim() + "," + ATMflds.TRANSACTION_END.Trim() + "," + ATMflds.REF2 + "," + ATMflds.CASHTAKENZ + Environment.NewLine;

                                    }
                                }
                                else
                                {
                                    if (scontent == "")
                                    {
                                        scontent = "CARD NO,DATE,AMOUNT,CURRENCY,UTRN NO,TRAN STAT,RC,AUTH NO,ATM NO,TRANSACTION_START,TRANSACTION_END,REFERENCE,CASH TAKEN" + Environment.NewLine;
                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.CURRENCY.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + "," + ATMflds.TRANSACTION_START.Trim() + "," + ATMflds.TRANSACTION_END.Trim() + "," + ATMflds.REF2 + "," + ATMflds.CASHTAKENZ + Environment.NewLine;

                                    }
                                    else
                                    {
                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.CURRENCY.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() +  "," + ATMflds.TRANSACTION_START.Trim() + "," + ATMflds.TRANSACTION_END.Trim() + "," + ATMflds.REF2 + "," + ATMflds.CASHTAKENZ + Environment.NewLine;

                                    }

                                }

                            }
                        }
                    }

                }
            }
            catch (Exception xc)
            {
                string sx = xc.Message;
            }
            //*****
            try
            {

                if (sGrp_s.Length != 0)
                {
                    for (int i = 1; i < sGrp_s.Length - 1; i++)
                    {
                        List<string> ly = this.GetJournalDetails_supervisor(sGrp_s[i].Split("\n"), ATMNO);
                        if (ly != null)
                        {

                            ATMflds.CARDNo = ly.Any(p => p.StartsWith("CARDNO:")) ? ly.First(p => p.StartsWith("CARDNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.trnDATE = ly.Any(p => p.StartsWith("DATE:")) ? ly.First(p => p.StartsWith("DATE:")).Replace("DATE:", "") : "";
                            ATMflds.AMOUNT = ly.Any(p => p.StartsWith("AMOUNT:")) ? ly.First(p => p.StartsWith("AMOUNT:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                            ATMflds.UTRNNO = ly.Any(p => p.StartsWith("SEQ:")) ? ly.First(p => p.StartsWith("SEQ:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.ReasonCode = ly.Any(p => p.StartsWith("RESPONSE CODE:")) ? ly.First(p => p.StartsWith("RESPONSE CODE:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.AtmNo = ly.Any(p => p.StartsWith("ATMNO:")) ? ly.First(p => p.StartsWith("ATMNO:")).Split(':')[1].ToString().Replace("|", "") : ATMNO;
                            ATMflds.AUTHNO = ly.Any(p => p.StartsWith("AUTHNO:")) ? ly.First(p => p.StartsWith("AUTHNO:")).Split(':')[1].ToString().Replace("|", "") : "";

                            ATMflds.AMOUNT_REMAINING = ly.Any(p => p.StartsWith("AMOUNTR:")) ? ly.First(p => p.StartsWith("AMOUNTR:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                            if (hascashcount == true)
                            {


                            }
                            if (hascashcount != true)
                            {
                                if (ATMflds.ReasonCode != "" && ATMflds.AMOUNT != "0" && ATMflds.SUCCESSFUL.Trim() == "APPROVED")
                                {
                                    if (scontent_sup == "")
                                    {

                                        scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                    else
                                    {
                                        scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                    if (ATMflds.AMOUNT_REMAINING != "0")
                                    {
                                        scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT_REMAINING + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + ",CASH LOADED AT ATM," + ATMflds.AUTHNO + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                        if (ATMflds.AMOUNT != ATMflds.AMOUNT_REMAINING)
                                        {
                                            scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + (Convert.ToDecimal(ATMflds.AMOUNT) - Convert.ToDecimal(ATMflds.AMOUNT_REMAINING)) + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + ",CASH REMAINING DIFFERENCE," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;
                                        }
                                    }
                                    hascashcount = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception x)
            {
                string df = x.Message;
            }
            //*****
            if (scontent_sup != "")
            {
                scontent_ += scontent;
                scontent_ += scontent_sup;
            }
            else
            {
                scontent_ += scontent;
            }

            outputFile = outputFolder + "\\Converted_ATMJournal_" + Path.GetFileNameWithoutExtension(inputFile) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".csv";
            WriteFile(outputFile, scontent_);


        }
        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }


        private List<string> GetJournalDetails(string[] d, string ATMNO = "")
        {
            bool gotcardno = false;
            bool gotdate = false;
            bool gotamount = false;
            bool gotcashtaken = false;
            bool gotcashtaken_n = false;
            bool gotRC = false;
            bool gotSTART = false;
            bool gotEND = false;
            bool gotSEQ = false;
            bool gotATM = false;
            bool gotATHNO = false;
            bool refusedtxn = false;
            bool gotref = false;
            List<string> l = new List<string>();
            string secondValue = "";
            for (int i = 1; i < d.Length - 1; i++)
            {

                try
                {
                    if (d[i].Contains("326501608962"))
                    {
                    }
                    if ((gotamount==false) && (d[i].Split(' ')[0].Split('/').Length>1))
                    {
                        if (d[i].Split(' ')[4] != "")
                        {
                            l.Add("AMOUNT:" + (Convert.ToDecimal(d[i].Split(' ')[4].Replace('+', ' ').Trim()).ToString()));
                        }
                        else if (d[i].Split(' ')[5] != "")
                        {
                            l.Add("AMOUNT:" + (Convert.ToDecimal(d[i].Split(' ')[5].Replace('+', ' ').Trim()).ToString()));
                        }

                        else
                        {
                            l.Add("AMOUNT:" + (Convert.ToDecimal(d[i].Split(' ')[8].Replace('+', ' ').Trim()).ToString()));
                        }
                       
                        gotamount = true;
                        l.Add("REFERENCE:" + d[i].Split(' ')[0]);
                        gotref = true;

                    }
                        if (d[i].Contains("TRANSACTION REPLY NEXT"))
                    {
                        if (d[i].Split(" ")[4]=="703"|| d[i].Split(" ")[4] == "396")
                        {
                            if (refusedtxn == false)
                            {
                                l.Add("REFUSED:" + d[i].Replace('\r', ' ').Replace(d[i].Split(" ")[0].ToString(), " ").Trim() + "|");
                                refusedtxn = true;
                            }
                        }
                        else if (d[i].Split(" ")[4] == "701")
                        { 
                            
                         }
                        else
                        {
                            l.Add("RESPONSE CODE:" + (d[i].Split(" ")[4]));
                            refusedtxn = true;
                        }
                        if (gotATHNO != true)
                        {
                            l.Add("AUTHNO:" + d[i].Split(" ")[4] + "|");
                            gotATHNO = true;

                        }



                    }

                    if (d[i].Contains("AMOUNT"))
                    {
                        if (d[i].Contains("ENTERED"))
                        {
                            if (gotamount != true)
                            {
                                l.Add("AMOUNT:" + (Convert.ToDecimal(d[i].Split(' ')[2]) / 100).ToString());
                                gotamount = true;

                            }
                            else
                            {
                            }
                        }
                    }
                    if (d[i].Contains("EMV AID"))
                    {
                        if (gotcardno != true)
                        {
                            l.Add("CARDNO:" + d[i].Split('/')[1].Replace("STARTED\r", " ").Trim() + "|");
                            gotcardno = true;
                        }
                        if(gotSEQ==false)
                        {
                            l.Add("SEQ:" + d[i].Split('/')[0].Split(' ')[3].Trim() + "|");
                            gotSEQ = true;
                        }
                    }
                    if (d[i].Contains("CARD"))
                    {
                        if (gotcardno != true)
                        {
                            if (d[i].Length > 22)
                                if (d[i].Contains(") TAKEN"))
                                {
                                    l.Add("CARDNO:" + d[i].Substring(14, 16) + "|");
                                    gotcardno = true;
                                }
                                else
                                {
                                    if (d[i].Contains("CARD NO:"))
                                    {
                                        if (d[i].Length > 35)
                                        {
                                            l.Add(d[i].Split("\r\n")[0] + "|");
                                            gotcardno = true;
                                        }
                                        else
                                        {
                                            l.Add("CARDNO:" + d[i].Split("\r\n")[0].Replace("CARD NO:", "") + "|");
                                            gotcardno = true;
                                        }

                                    }
                                }
                        }
                    }
                    //CASH TAKEN
                    if (d[i].Contains("CASH TAKEN"))
                    {
                        if (gotcashtaken_n != true)
                        {
                            l.Add("CASH TAKENZ| " + d[i].Split(' ')[0]);
                            gotcashtaken_n = true;
                        }
                    }
                        if (d[i].Contains("ATM NUMBER:"))
                    {
                        if (gotdate != true)
                        {
                            if (d[i + 1].ToString().Length < 45)
                            {
                               
                                if (gotATM != true)
                                {
                                    try
                                    {
                                        l.Add("ATMNO:" + d[i].Substring(15, 8));
                                        gotATM = true;
                                    }
                                    catch (Exception)
                                    {
                                        l.Add("ATMNO:" + ATMNO);
                                    }

                                }
                            }
                            else
                            {
                                l.Add(d[i].ToString());
                                gotdate = true;
                            }
                        }
                    }
                    if (d[i].Contains("******************************"))
                    {
                        if (gotdate != true)
                        {
                            if (d[i].ToString().Length < 45)
                            {
                                l.Add("DATE:" + d[i + 1].Split(' ')[1] + " " + d[i + 1].Split(' ')[2].Replace("\r", ""));
                                gotdate = true;
                            }
                            else
                            {
                                l.Add(d[i].ToString());
                                gotdate = true;
                            }

                        }
                        if (gotATM != true)
                        {
                            l.Add("ATMNO:" + d[i + 1].Split(' ')[0]);
                            gotATM = true;

                        }

                    }
                    if (d[i].Contains("AMOUNT"))
                    {
                        if (d[i].Contains("ENTERED"))
                        { 
                            if (gotamount != true)
                        {
                            l.Add("AMOUNT:" + (Convert.ToDecimal(d[i].Split(' ')[2]) / 100).ToString());
                            gotamount = true;

                        }
                            else 
                            {
                            }
                        }
                    }


                    if (d[i].Contains("ATM:"))
                    {
                        if (gotATM != true)
                        {
                            l.Add("ATMNO:" + d[i].Replace("ATM:", "").Split(" ")[0].Trim());
                            gotATM = true;

                        }
                    }


                    if (d[i].Contains("<CASH_WITHDRAWAL>"))
                    {
                        if(gotRC==false)
                        {
                            l.Add("RESPONSE CODE:" + d[i].Split('<')[0].Trim().Replace(","," "));
                            gotRC = true;
                        }
                
                    }

                    if (d[i].Contains("TRACK 2 DATA"))
                    {
                        if (gotSTART == false)
                        {
                            l.Add("TRANSACTION_START|" + d[i].Split(' ')[0]);
                            gotSTART = true;
                        }

                    }

                    if (d[i].Contains("TRANSACTION END"))
                    {
                        if (gotEND == false)
                        {
                            l.Add("TRANSACTION_END|" + d[i].Split(' ')[0]);
                            gotEND = true;
                        }

                    }
                    if (d[i].Contains("CASH PRESENTED"))
                    {
                        l.Add("RESPONSE CODE: CASH PRESENTED");
                        gotRC = true;
                    }
                        if (d[i].Contains("TRANSACTION REPLY NEXT 701 FUNCTION A192"))
                    {
                        if (gotcashtaken != true)
                        {
                            if (d[i].Split("\r\n")[0].Length < 20)
                            {
                                l.Add(d[i].Split("\r\n")[0].Substring(9, 10) + "|");
                                gotcashtaken = true;
                            }
                            else
                            {
                                l.Add(d[i].Split("\r\n")[0] + "|");
                                gotcashtaken = true;
                            }
                        }
                        if (gotATHNO != true)
                        {
                            l.Add("AUTHNO:" + d[i].Split(' ')[4] + "|");
                            gotATHNO = true;

                        }

                    }
                    if (d[i].Contains("CASH PRESENTED"))
                    {
                        if (gotRC != true)
                        {
                            l.Add(d[i].Split("\r\n")[0] + "|");
                            if (gotcashtaken == false)
                            {
                                if (d[i].Split(":")[1].Trim() == "1")
                                {
                                    l.Add("CASH TAKEN:" + d[i].Split("\r\n")[0] + "|");
                                    gotcashtaken = true;
                                }
                            }

                            gotRC = true;
                        }

                    }

                    //
                    if (d[i].Contains("CASH_WITHDRAWAL"))
                    {
                        try
                        {
                            secondValue = d[i + 1];
                         
                            if (int.TryParse(secondValue.Split(' ')[0].Split('/')[0], out int intValue) || double.TryParse(secondValue.Split(' ')[0].Split('/')[0], out double doubleValue))
                            {
                                l.Add("CCY:" + d[i + 1].Split(' ')[d[i + 1].Split(' ').Length - 1].Replace("\r", ""));
                                l.Add("REFERENCE:" + d[i + 1].Split(' ')[0]);
                            }
                            else
                            {

                            }
                        }
                        catch (Exception xc) { }
                       
                      
                    }
                        if (d[i].Contains("SEQ:"))
                    {
                        if (gotSEQ != true)
                        {
                            if (d[i].Split("\r\n")[0].Length < 45)
                            {
                                l.Add(d[i].Split("\r\n")[0].Split(" ")[2] + "|");
                                gotSEQ = true;
                            }
                            else
                            {
                                l.Add(d[i].Split("\r\n")[0] + "|");
                                gotSEQ = true;
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    string err = e.Message;
                }

            }

            return l;
        }
        private List<string> GetJournalDetails_supervisor(string[] d, string ATMNo_ = "")
        {
            bool gotdate = false;
            bool gotdate_s = false;
            bool gotamount = false;
            bool gotamount_s = false;
            bool gotcashtaken = false;
            bool gotcashtaken_n = false;
            bool gotcashtaken_s = false;
            bool gotRC = false;
            bool gotATM = false;
            bool gotATM_s = false;
            decimal type1 = 0;
            decimal type2 = 0;
            decimal type3 = 0;
            decimal type4 = 0;

            string currenAMount = "";
            string currenAMount_ = "";

            string currenAMount_s = "";
            string currenAMount_x = "";

            decimal type1_s = 0;
            decimal type2_s = 0;
            decimal type3_s = 0;
            decimal type4_s = 0;

            List<string> l = new List<string>();
            for (int i = 1; i < d.Length - 1; i++)
            {//CARD	DATE	AMOUNT	UTRN NO	SUCCESSFUL	RC CARD NO:



                if (d[i].Contains("CASH COUNTS CLEARED"))
                {
                    l.Add("CASH COUNTS CLEARED");
                    //return l;
                }
                if (d[i].Contains("CASH ADDED"))
                {
                    if (gotATM_s != true)
                    {
                        //get date
                        if (gotdate != true)
                        {
                            try { l.Add("DATE:" + d[i + 3].Split('*')[2].Substring(3, 2) + "/" + d[i + 3].Split('*')[2].Substring(0, 2) + "/" + d[i + 3].Split('*')[2].Substring(8, 2) + " " + d[i + 3].Split('*')[3]); }
                            catch (Exception)
                            {
                                try
                                {
                                    l.Add("DATE:" + d[i + 4].Split('*')[2].Substring(3, 2) + "/" + d[i + 4].Split('*')[2].Substring(0, 2) + "/" + d[i + 4].Split('*')[2].Substring(8, 2) + " " + d[i + 4].Split('*')[3]);
                                }
                                catch (Exception)
                                {
                                    try
                                    {
                                        l.Add("DATE:" + d[i + 4].Split('=')[1].Substring(3, 2) + "/" + d[i + 4].Split('=')[1].Substring(0, 2) + "/" + d[i + 4].Split('=')[1].Substring(8, 2) + " " + d[i + 4].Split('=')[1].Substring(9, 5));
                                    }
                                    catch (Exception)
                                    {
                                        l.Add("DATE:" + d[i + 5].Split('=')[1].Substring(3, 2) + "/" + d[i + 5].Split('=')[1].Substring(0, 2) + "/" + d[i + 5].Split('=')[1].Substring(8, 2) + " " + d[i + 5].Split('=')[1].Substring(9, 5));
                                    }

                                }


                            }

                            gotdate = true;
                        }
                        //get amount
                        if (d[i + 1].Trim() != currenAMount && currenAMount != "")
                        {
                            l.Remove(currenAMount_);
                            gotamount = false;
                        }
                        if (gotamount != true)
                        {
                            try
                            { type1 = Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('=')[1].Trim()) * 1000; }
                            catch (Exception)
                            { type1 = Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('≈')[1].Trim()) * 1000; }
                            try
                            {
                                if ((ATMNo_ == "ATN07013") || (ATMNo_ == "ATN07006") || (ATMNo_ == "ATN07008") || (ATMNo_ == "ATN07025") || (ATMNo_ == "ATN07106") || (ATMNo_ == "ATW07012") || (ATMNo_ == "ATW07024") || (ATMNo_ == "ATN07001") || (ATMNo_ == "ATW07018"))
                                {
                                    type2 = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('=')[1].Trim()) * 2000;
                                }
                                else
                                { type2 = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('=')[1].Trim()) * 5000; }

                            }
                            catch (Exception)
                            {

                                if ((ATMNo_ == "ATN07013") || (ATMNo_ == "ATN07006") || (ATMNo_ == "ATN07008") || (ATMNo_ == "ATN07025") || (ATMNo_ == "ATN07106") || (ATMNo_ == "ATW07012") || (ATMNo_ == "ATW07024") || (ATMNo_ == "ATN07001") || (ATMNo_ == "ATW07018"))
                                {
                                    type2 = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('≈')[1].Trim()) * 2000;
                                }
                                else
                                { type2 = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('≈')[1].Trim()) * 5000; }
                            }
                            try
                            { type3 = Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('=')[1].Trim()) * 5000; }
                            catch (Exception)
                            { type3 = Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('≈')[1].Trim()) * 5000; }

                            try
                            { type4 = Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('=')[1].Trim()) * 5000; }
                            catch (Exception)
                            { type4 = Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('≈')[1].Trim()) * 5000; }

                            l.Add("AMOUNT" + (type1 + type2 + type3 + type4));
                            currenAMount_ = "AMOUNT" + (type1 + type2 + type3 + type4);
                            currenAMount = d[i + 1].Trim();
                            gotamount = true;
                        }
                        if (gotcashtaken == false)
                        {
                            l.Add("CASH TAKEN:" + d[i].Split("\r\n")[0] + "|");
                            gotcashtaken = true;
                        }
                        if (gotRC != true)
                        {
                            l.Add("RESPONSE CODE:" + d[i].Split("\r\n")[0] + "|");
                            gotRC = true;
                        }

                        gotATM = true;

                    }
                }


                if (d[i].Contains("CASH REMAINING"))
                {
                    if (gotATM != true)
                    {
                        //get date
                        if (gotdate_s != true)
                        {

                            gotdate_s = true;
                        }
                        //get amount
                        if (d[i + 1].Trim() != currenAMount_s && currenAMount_s != "")
                        {
                            l.Remove(currenAMount_x);
                            gotamount = false;
                        }
                        if (gotamount_s != true)
                        {
                            try
                            { type1_s = Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('=')[1].Trim()) * 1000; }
                            catch (Exception)
                            { type1_s = Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('≈')[1].Trim()) * 1000; }
                            try
                            { type2_s = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('=')[1].Trim()) * 2000; }
                            catch (Exception)
                            {
                                type2_s = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('≈')[1].Trim()) * 2000;
                            }
                            try
                            { type3_s = Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('=')[1].Trim()) * 5000; }
                            catch (Exception)
                            { type3_s = Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('≈')[1].Trim()) * 5000; }

                            try
                            { type4_s = Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('=')[1].Trim()) * 5000; }
                            catch (Exception)
                            { type4_s = Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('≈')[1].Trim()) * 5000; }

                            l.Add("AMOUNTR:" + (type1_s + type2_s + type3_s + type4_s));
                            currenAMount_x = "AMOUNTR:" + (type1_s + type2_s + type3_s + type4_s);
                            currenAMount_s = d[i + 1].Trim();
                            gotamount_s = true;
                        }
                        if (gotcashtaken_s == false)
                        {
                            l.Add("AMOUNT REMAINING:" + d[i].Split("\r\n")[0] + "|");
                            gotcashtaken_s = true;
                        }

                        gotATM = true;

                    }
                }
            }




            return l;
        }
        public class ATMJournal
        {
            public string CARDNo { get; set; }
            public string trnDATE { get; set; }
            public string AMOUNT { get; set; }
            public string UTRNNO { get; set; }
            public string SUCCESSFUL { get; set; }
            public string ReasonCode { get; set; }

            public string AMOUNT_REMAINING { get; set; }

            public string AtmNo { get; set; }
            public string AUTHNO { get; set; }

            public string CURRENCY { get; set; }

            public string TRANSACTION_START { get; set; }

            public string CASHTAKENZ { get; set; }           

            public string TRANSACTION_END { get; set; }

            public string  REF2 { get; set; }

        public Boolean Cashtaken { get; set; }

        }
        public void ConvertFile_NCR(string inputFile)
        {

            string ATMNO = "";
            string outputFolder = null;

            string outputFile = "";

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
            }

            outputFolder = Path.GetFullPath(Path.Combine(outputFolder, @"..\")) + "Conv";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            string[] sDet = File.ReadAllLines(inputFile);
            try
            {
                if (sDet[0].Length != 0)
                {
                    ATMNO = sDet[0].Split(" ")[0].Trim();
                }
            }
            catch (Exception)
            { }
            string content = File.ReadAllText(inputFile);


            string[] sGrp = content.Split("TRANSACTION START");
            string[] sGrp_s = content.Split("SUPERVISOR MODE ENTRY");
            string scontent = "";
            string scontent_ = "";
            string scontent_sup = "";
            ATMJournal ATMflds = new ATMJournal();

            bool hascashcount = false;

            try
            {
                if (sGrp.Length != 0)
                {

                    for (int i = 1; i < sGrp.Length - 1; i++)
                    {

                        List<string> lx = this.GetJournalDetails_NCR(sGrp[i].Split("\r"), ATMNO);
                        if (lx.Count > 0)
                        {
                            ATMflds.CARDNo = lx.Any(p => p.StartsWith("CARDNO:")) ? lx.First(p => p.StartsWith("CARDNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.trnDATE = lx.Any(p => p.StartsWith("DATE:")) ? lx.First(p => p.StartsWith("DATE:")).Replace("DATE:", "") : "";
                            ATMflds.AMOUNT = lx.Any(p => p.StartsWith("AMOUNT:")) ? lx.First(p => p.StartsWith("AMOUNT:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                            ATMflds.UTRNNO = lx.Any(p => p.StartsWith("SEQ:")) ? lx.First(p => p.StartsWith("SEQ:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.ReasonCode = lx.Any(p => p.StartsWith("RESPONSE CODE:")) ? lx.First(p => p.StartsWith("RESPONSE CODE:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.AtmNo = lx.Any(p => p.StartsWith("ATMNO:")) ? lx.First(p => p.StartsWith("ATMNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.AUTHNO = lx.Any(p => p.StartsWith("AUTHNO:")) ? lx.First(p => p.StartsWith("AUTHNO:")).Split(':')[1].ToString().Replace("|", "") : "";

                            ATMflds.Cashtaken = lx.Any(p => p.StartsWith("CASH TAKEN")) ? true : false;

                            if (ATMflds.Cashtaken == true)
                            {
                                ATMflds.SUCCESSFUL = "APPROVED";
                            }
                            else
                            {
                                ATMflds.SUCCESSFUL = "DECLINED";
                            }
                            if (ATMflds.CARDNo != "" && ATMflds.AMOUNT != "0")
                            {
                                ATMNO = ATMflds.AtmNo;
                                if (scontent_sup != "")
                                {

                                    if (scontent == "")
                                    {

                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                    else
                                    {
                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                }
                                else
                                {
                                    if (scontent == "")
                                    {
                                        scontent = "CARD NO, DATE ,   AMOUNT,UTRN NO ,TRAN STAT,RC,AUTH NO,ATM NO" + Environment.NewLine;
                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                    else
                                    {
                                        scontent += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT.Trim() + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL.Trim() + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }

                                }

                            }
                        }
                    }

                }
            }
            catch (Exception xc)
            {
                string sx = xc.Message;
            }
            //*****
            try
            {

                if (sGrp_s.Length != 0)
                {
                    for (int i = 1; i < sGrp_s.Length - 1; i++)
                    {
                        List<string> ly = this.GetJournalDetails_supervisor(sGrp_s[i].Split("\n"), ATMNO);
                        if (ly != null)
                        {

                            ATMflds.CARDNo = ly.Any(p => p.StartsWith("CARDNO:")) ? ly.First(p => p.StartsWith("CARDNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.trnDATE = ly.Any(p => p.StartsWith("DATE:")) ? ly.First(p => p.StartsWith("DATE:")).Replace("DATE:", "") : "";
                            ATMflds.AMOUNT = ly.Any(p => p.StartsWith("AMOUNT:")) ? ly.First(p => p.StartsWith("AMOUNT:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                            ATMflds.UTRNNO = ly.Any(p => p.StartsWith("SEQ:")) ? ly.First(p => p.StartsWith("SEQ:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.ReasonCode = ly.Any(p => p.StartsWith("RESPONSE CODE:")) ? ly.First(p => p.StartsWith("RESPONSE CODE:")).Split(':')[1].ToString().Replace("|", "") : "";
                            ATMflds.AtmNo = ly.Any(p => p.StartsWith("ATMNO:")) ? ly.First(p => p.StartsWith("ATMNO:")).Split(':')[1].ToString().Replace("|", "") : ATMNO;
                            ATMflds.AUTHNO = ly.Any(p => p.StartsWith("AUTHNO:")) ? ly.First(p => p.StartsWith("AUTHNO:")).Split(':')[1].ToString().Replace("|", "") : "";

                            ATMflds.AMOUNT_REMAINING = ly.Any(p => p.StartsWith("AMOUNTR:")) ? ly.First(p => p.StartsWith("AMOUNTR:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                            if (hascashcount == true)
                            {


                            }
                            if (hascashcount != true)
                            {
                                if (ATMflds.ReasonCode != "" && ATMflds.AMOUNT != "0" && ATMflds.SUCCESSFUL.Trim() == "APPROVED")
                                {
                                    if (scontent_sup == "")
                                    {

                                        scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                    else
                                    {
                                        scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode.Trim() + "," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                    }
                                    if (ATMflds.AMOUNT_REMAINING != "0")
                                    {
                                        scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + ATMflds.AMOUNT_REMAINING + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + ",CASH LOADED AT ATM," + ATMflds.AUTHNO + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;

                                        if (ATMflds.AMOUNT != ATMflds.AMOUNT_REMAINING)
                                        {
                                            scontent_sup += ATMflds.CARDNo.Trim() + "," + ATMflds.trnDATE.Trim() + "," + (Convert.ToDecimal(ATMflds.AMOUNT) - Convert.ToDecimal(ATMflds.AMOUNT_REMAINING)) + "," + ATMflds.UTRNNO.Trim() + "," + ATMflds.SUCCESSFUL + ",CASH REMAINING DIFFERENCE," + ATMflds.AUTHNO.Trim() + "," + ATMflds.AtmNo.Trim() + Environment.NewLine;
                                        }
                                    }
                                    hascashcount = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception x)
            {
                string df = x.Message;
            }
            //*****
            if (scontent_sup != "")
            {
                scontent_ += scontent;
                scontent_ += scontent_sup;
            }
            else
            {
                scontent_ += scontent;
            }

            outputFile = outputFolder + "\\Converted_ATMJournal_" + Path.GetFileNameWithoutExtension(inputFile) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".csv";
            WriteFile(outputFile, scontent_);


        }

        private List<string> GetJournalDetails_NCR(string[] d, string ATMNO = "")
        {
            bool gotcardno = false;
            bool gotdate = false;
            bool gotamount = false;
            bool gotcashtaken = false;
            bool gotRC = false;
            bool gotSEQ = false;
            bool gotATM = false;
            bool gotATHNO = false;
            bool refusedtxn = false;
            List<string> l = new List<string>();
            for (int i = 1; i < d.Length - 1; i++)
            {

                try
                {
                    if (d[i].Contains("REFUSED TRANSACTION"))
                    {
                        if (refusedtxn == false)
                        {
                            l.Add("REFUSED:" + (d[i]));
                            refusedtxn = true;
                        }


                    }
                    if (d[i].Contains("WITHDRAWAL ("))
                    {
                        if (gotcardno != true)
                        {
                            l.Add("CARDNO:" + d[i].Substring(0, 16) + "|");
                            gotcardno = true;
                        }
                    }
                    if (d[i].Contains("CARD"))
                    {
                        if (gotcardno != true)
                        {
                            if (d[i].Length > 22)
                                if (d[i].Contains(") TAKEN"))
                                {
                                    l.Add("CARDNO:" + d[i].Substring(14, 16) + "|");
                                    gotcardno = true;
                                }
                                else
                                {
                                    if (d[i].Contains("CARD NO:"))
                                    {
                                        if (d[i].Length > 35)
                                        {
                                            l.Add(d[i].Split("\r\n")[0] + "|");
                                            gotcardno = true;
                                        }
                                        else
                                        {
                                            l.Add("CARDNO:" + d[i].Split("\r\n")[0].Replace("CARD NO:", "") + "|");
                                            gotcardno = true;
                                        }

                                    }
                                }
                        }
                    }

                    if (d[i].Contains("ATN"))
                    {
                        if (gotdate != true)
                        {
                            if (d[i + 1].ToString().Length < 45)
                            {
                                l.Add("DATE:" + d[i].Substring(0, 8).Replace(".", "/") + " " + d[i].Substring(9, 5));
                                gotdate = true;
                                if (gotATM != true)
                                {
                                    try
                                    {
                                        l.Add("ATMNO:" + d[i].Substring(15, 8));
                                        gotATM = true;
                                    }
                                    catch (Exception)
                                    {
                                        l.Add("ATMNO:" + ATMNO);
                                    }

                                }
                            }
                            else
                            {
                                l.Add(d[i].ToString());
                                gotdate = true;
                            }
                        }
                    }
                    if (d[i].Contains("DATE:"))
                    {
                        if (gotdate != true)
                        {
                            if (d[i].ToString().Length < 45)
                            {
                                l.Add("DATE:" + d[i].Substring(5, 8).Replace(".", "/") + " " + d[i].Substring(19, 5));
                                gotdate = true;
                            }
                            else
                            {
                                l.Add(d[i].ToString());
                                gotdate = true;
                            }

                        }

                    }
                    if (d[i].Contains("AMOUNT:"))
                    {
                        if (gotamount != true)
                        {
                            l.Add(d[i]);
                            gotamount = true;

                        }
                    }


                    if (d[i].Contains("ATM:"))
                    {
                        if (gotATM != true)
                        {
                            l.Add("ATMNO:" + d[i].Replace("ATM:", "").Split(" ")[0].Trim());
                            gotATM = true;

                        }
                    }


                    if (d[i].Contains("AUTH. NO:"))
                    {
                        if (gotATHNO != true)
                        {
                            l.Add("AUTHNO:" + d[i].Split(':')[1].Trim() + "|");
                            gotATHNO = true;

                        }
                    }
                    if (d[i].Contains("CASH TAKEN"))
                    {
                        if (gotcashtaken != true)
                        {
                            if (d[i].Split("\r\n")[0].Length < 20)
                            {
                                l.Add(d[i].Split("\r\n")[0].Substring(9, 10) + "|");
                                gotcashtaken = true;
                            }
                            else
                            {
                                l.Add(d[i].Split("\r\n")[0] + "|");
                                gotcashtaken = true;
                            }
                        }

                    }
                    if (d[i].Contains("RESPONSE CODE:"))
                    {
                        if (gotRC != true)
                        {
                            l.Add(d[i].Split("\r\n")[0] + "|");
                            if (gotcashtaken == false)
                            {
                                if (d[i].Split(":")[1].Trim() == "1")
                                {
                                    l.Add("CASH TAKEN:" + d[i].Split("\r\n")[0] + "|");
                                    gotcashtaken = true;
                                }
                            }

                            gotRC = true;
                        }

                    }


                    if (d[i].Contains("SEQ:"))
                    {
                        if (gotSEQ != true)
                        {
                            if (d[i].Split("\r\n")[0].Length < 45)
                            {
                                l.Add(d[i].Split("\r\n")[0].Split(" ")[2] + "|");
                                gotSEQ = true;
                            }
                            else
                            {
                                l.Add(d[i].Split("\r\n")[0] + "|");
                                gotSEQ = true;
                            }
                        }

                    }
                }
                catch (Exception e)
                {
                    string err = e.Message;
                }

            }

            return l;
        }
    }
}
