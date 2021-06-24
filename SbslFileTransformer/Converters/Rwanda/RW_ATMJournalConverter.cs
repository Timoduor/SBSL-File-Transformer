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
            string outputFolder = null;
            if (string.IsNullOrEmpty(outputFolder)) outputFolder = Path.GetDirectoryName(inputFile);
            string[] sDet = File.ReadAllLines(inputFile);
            string content = File.ReadAllText(inputFile);
            string[] sGrp = content.Split("DATE    TIME   ATM   OPERATION");
            string scontent = "";

            if (sGrp.Length != 0)
            {
                var ATMflds = new ATMJournal();
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
                        if (ATMflds.CARDNo != "")
                        {
                            if (scontent == "")
                            {
                                scontent = "CARD NO, DATE ,   AMOUNT,UTRN NO ,TRAN STAT,RC,AUTH NO,ATM NO" + Environment.NewLine;
                                scontent += ATMflds.CARDNo + "," + ATMflds.trnDATE + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode + "," + ATMflds.AUTHNO + "," + ATMflds.AtmNo + Environment.NewLine;
                                //lx[1].Split(':')[4].Replace("|","")

                            }
                            else
                            {
                                scontent += ATMflds.CARDNo + "," + ATMflds.trnDATE + "," + ATMflds.AMOUNT + "," + ATMflds.UTRNNO + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode + "," + ATMflds.AUTHNO + "," + ATMflds.AtmNo + Environment.NewLine;

                            }
                        }
                    }
                }
                WriteFile(outputFolder + "\\Converted_ATMjrn_" + Path.GetFileNameWithoutExtension(inputFile) + ".csv", scontent);

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
            bool gotamount = false;
            bool gotcashtaken = false;
            bool gotRC = false;
            bool gotSEQ = false;
            bool gotATM = false;
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


                if (d[i].Contains("ATM55:"))
                {
                    if (gotATM != true)
                    {
                        l.Add(d[i]);
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
                            if (d[i].Split(":")[1] == "1")
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
                        if (d[i].Split("\r\n")[0].Length < 40)
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
