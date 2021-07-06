using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class RW_ATMJournalConverter
    {

        public void ConvertFile_WinkaATMjrn(string inputFile)
        {

            string ATMNO = "";
            string outputFolder = null;
             
            string outputFile = "";

            string[] sDet = File.ReadAllLines(inputFile);
            try
            {
                if (sDet[0].Length != 0)
                {
                    ATMNO = sDet[0].Split(" ")[0].Trim();
                }
            }
            catch (Exception xc)
            { }
            string content = File.ReadAllText(inputFile);
            string[] sGrp = content.Split("DATE    TIME   ATM   OPERATION");
            string[] sGrp_s = content.Split("SUPERVISOR MODE ENTRY");
            string scontent = "";
            string scontent_ = "";
            string scontent_sup = "";
            var ATMflds = new ATMJournal();
           
            bool hascashcount = false;
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
            }

            if (sGrp_s.Length != 0)
            {
                for (var i = 1; i < sGrp_s.Length - 1; i++)
                {
                    List<string> ly = GetJournalDetails_supervisor(sGrp_s[i].Split("\n"));
                    if (ly.Count > 0)
                    {
                        //hascashcount = ly.Any(p => p.StartsWith("CASH COUNTS CLEARED")) ? true : false; //"CASH COUNTS CLEARED"
                        ATMflds.CARDNo = ly.Any(p => p.StartsWith("CARDNO:")) ? ly.First(p => p.StartsWith("CARDNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                        ATMflds.trnDATE = ly.Any(p => p.StartsWith("DATE:")) ? ly.First(p => p.StartsWith("DATE:")).Replace("DATE:", "") : "";
                        ATMflds.AMOUNT = ly.Any(p => p.StartsWith("AMOUNT:")) ? ly.First(p => p.StartsWith("AMOUNT:")).Split(":")[1].Trim().Split(" ")[0] : "0";
                        ATMflds.UTRNNO = ly.Any(p => p.StartsWith("SEQ:")) ? ly.First(p => p.StartsWith("SEQ:")).Split(':')[1].ToString().Replace("|", "") : "";
                        ATMflds.ReasonCode = ly.Any(p => p.StartsWith("RESPONSE CODE:")) ? ly.First(p => p.StartsWith("RESPONSE CODE:")).Split(':')[1].ToString().Replace("|", "") : "";
                        ATMflds.AtmNo = ly.Any(p => p.StartsWith("ATMNO:")) ? ly.First(p => p.StartsWith("ATMNO:")).Split(':')[1].ToString().Replace("|", "") : ATMNO;
                        ATMflds.AUTHNO = ly.Any(p => p.StartsWith("AUTHNO:")) ? ly.First(p => p.StartsWith("AUTHNO:")).Split(':')[1].ToString().Replace("|", "") : "";

                        if (hascashcount == true)
                        {


                        }
                        if (hascashcount != true)
                        {
                            if (ATMflds.ReasonCode != "" && ATMflds.AMOUNT != "0")
                            {
                                if (scontent_sup == "")
                                {
                                    scontent_sup = "CARD NO, DATE ,   AMOUNT,UTRN NO ,TRAN STAT,RC,AUTH NO,ATM NO" + Environment.NewLine;
                                    scontent_sup += ATMflds.CARDNo + "," + ATMflds.trnDATE + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode + "," + ATMflds.AUTHNO + "," + ATMflds.AtmNo + Environment.NewLine;

                                }
                                else
                                {
                                    scontent_sup += ATMflds.CARDNo + "," + ATMflds.trnDATE + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode + "," + ATMflds.AUTHNO + "," + ATMflds.AtmNo + Environment.NewLine;

                                }
                                hascashcount = false;
                            }
                        }
                    }
                }
            }

            if (sGrp.Length != 0)
            {

                for (var i = 1; i < sGrp.Length - 1; i++)
                {
                    List<string> lx = GetJournalDetails(sGrp[i].Split("\n"));
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
                            if (scontent_sup != "")
                            {

                                if (scontent == "")
                                {
                                    //scontent = "CARD NO, DATE ,   AMOUNT,UTRN NO ,TRAN STAT,RC,AUTH NO,ATM NO" + Environment.NewLine;
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
                if (scontent_sup != "")
                {
                    scontent_ += scontent_sup;
                    scontent_ += scontent;
                }
                else
                {
                    scontent_ += scontent;
                }
                outputFile = outputFolder + "\\Converted_ATMJournal_" + Path.GetFileNameWithoutExtension(inputFile) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".csv";
                WriteFile(outputFile, scontent_);

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

        private List<string> GetJournalDetails(string[] d)
        {
            bool gotcardno = false;
            bool gotdate = false;
            bool gotdate_s = false;
            bool gotamount = false;
            bool gotamount_s = false;
            bool gotcashtaken = false;
            bool gotcashtaken_s = false;
            bool gotRC = false;
            bool gotSEQ = false;
            bool gotATM = false;
            bool gotATM_s = false;
            bool gotATHNO = false;
            bool refusedtxn = false;

            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {//CARD	DATE	AMOUNT	UTRN NO	SUCCESSFUL	RC CARD NO:
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
                //DATE    TIME   ATM   OPERATION
                if (d[i].Contains("ATN"))
                {
                    if (gotdate != true)
                    {
                        if (d[i + 1].ToString().Length < 40)
                        {
                            l.Add("DATE:" + d[i].Substring(0, 8).Replace(".", "/") + " " + d[i].Substring(9, 5));
                            gotdate = true;
                            if (gotATM != true)
                            {
                                l.Add("ATMNO:" + d[i].Substring(15, 8));
                                gotATM = true;

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
                        if (d[i].ToString().Length < 40)
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

            return l;
        }
        private List<string> GetJournalDetails_supervisor(string[] d)
        {
            bool gotcardno = false;
            bool gotdate = false;
            bool gotdate_s = false;
            bool gotamount = false;
            bool gotamount_s = false;
            bool gotcashtaken = false;
            bool gotcashtaken_s = false;
            bool gotRC = false;
            bool gotSEQ = false;
            bool gotATM = false;
            bool gotATM_s = false;
            bool gotATHNO = false;
            bool refusedtxn = false;
            decimal type1 = 0;
            decimal type2 = 0;
            decimal type3 = 0;
            decimal type4 = 0;


            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {//CARD	DATE	AMOUNT	UTRN NO	SUCCESSFUL	RC CARD NO:

                if (d[i].Contains("*424*06"))
                {

                }
                if (d[i].Contains("*452*06"))
                {

                }
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
                        if (gotdate_s != true)
                        {
                            l.Add("DATE:" + d[i + 3].Split('*')[2].Substring(3, 2) + "/" + d[i + 3].Split('*')[2].Substring(0, 2) + "/" + d[i + 3].Split('*')[2].Substring(6, 4) + " " + d[i + 3].Split('*')[3]);
                            gotdate_s = true;
                        }
                        //get amount
                        if (gotamount_s != true)
                        {
                            try
                            { type1 = Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('=')[1].Trim()) * 1000; }
                            catch (Exception xc)
                            { type1 = Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('≈')[1].Trim()) * 1000; }
                            try
                            { type2 = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('=')[1].Trim()) * 5000; }
                            catch (Exception xc)
                            {
                                type2 = Convert.ToDecimal(d[i + 1].Split("TYPE")[2].Split('≈')[1].Trim()) * 5000;
                            }
                            try
                            { type3 = Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('=')[1].Trim()) * 5000; }
                            catch (Exception xc)
                            { type3 = Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('≈')[1].Trim()) * 5000; }

                            try
                            { type4 = Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('=')[1].Trim()) * 5000; }
                            catch (Exception xc)
                            { type4 = Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('≈')[1].Trim()) * 5000; }

                            l.Add("AMOUNT:" + (type1 + type2 + type3 + type3));
                            // l.Add("AMOUNT:" + (((Convert.ToDecimal(d[i + 1].Split("TYPE")[1].Split('=')[1].Trim())) + (((Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('=')[2].Trim())) * 5000)) + (Convert.ToDecimal(d[i + 2].Split("TYPE")[1].Split('=')[1].Trim()) * 5000) * (Convert.ToDecimal(d[i + 2].Split("TYPE")[2].Split('=')[1].Trim()) * 5000)));
                            gotamount_s = true;
                        }
                        if (gotcashtaken == false)
                        {
                            l.Add("CASH TAKEN:" + d[i].Split("\r\n")[0] + "|");

                        }
                        if (gotRC != true)
                        {
                            l.Add("RESPONSE CODE:" + d[i].Split("\r\n")[0] + "|");
                            gotRC = true;
                        }
                        //RESPONSE CODE:
                        //l.Add(d[i]);
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
            public string AtmNo { get; set; }
            public string AUTHNO { get; set; }
            public Boolean Cashtaken { get; set; }

        }

    }
}
