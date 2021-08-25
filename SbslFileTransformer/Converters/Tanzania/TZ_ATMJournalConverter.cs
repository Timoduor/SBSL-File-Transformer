using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class TZ_ATMJournalConverter
    {
        public void ProcessATMjournalFile(string inputFile)
        {
            string outputFile = "";
            string outputFolder = null;

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(inputFile);
            }
            outputFolder = Path.GetFullPath(Path.Combine(outputFolder, @"..\")) + "Conv";
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);


            string[] sDet = File.ReadAllLines(inputFile);
            string content = File.ReadAllText(inputFile);
            string[] sGrp = content.Split("-> TRANSACTION START");
            string scontent = "";

            if (sGrp.Length != 0)
            {
                var ATMflds = new ATMJournal();
                for (var i = 1; i < sGrp.Length; i++)
                {
                    List<string> lx = GetJournalDetails(sGrp[i].Split("\r\n"));
                    if (lx.Count > 0)
                    {
                        ATMflds.CARDNo = lx.Any(p => p.StartsWith("CARDNO:")) ? lx.First(p => p.StartsWith("CARDNO:")).Split(':')[1].ToString().Replace("|", "") : "";
                        ATMflds.trnDATE = lx.Any(p => p.StartsWith("DATE:")) ? lx.First(p => p.StartsWith("DATE:")).Replace("DATE:", "") : "";
                        ATMflds.AMOUNT = lx.Any(p => p.StartsWith("AMOUNT:")) ? lx.First(p => p.StartsWith("AMOUNT:")).Split(":")[1].Trim().Split(" ")[1].Replace(",", "") : "0";
                        ATMflds.UTRNNO = lx.Any(p => p.StartsWith("SEQ:")) ? lx.First(p => p.StartsWith("SEQ:")).Split(':')[1].ToString().Replace("|", "") : "";
                        ATMflds.ReasonCode = lx.Any(p => p.StartsWith("DESC:")) ? lx.First(p => p.StartsWith("DESC:")).Split(':')[1].ToString().Replace("|", "") : "";
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
                                scontent = "CARD, DATE ,   AMOUNT, UTRN NO ,SUCCESSFUL,  RC,ATM NO" + Environment.NewLine;
                                scontent += ATMflds.CARDNo + ",'" + ATMflds.trnDATE + "," + ATMflds.AMOUNT + ",'" + ATMflds.UTRNNO + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode + "," + ATMflds.AtmNo + Environment.NewLine;

                            }
                            else
                            {
                                scontent += ATMflds.CARDNo + ",'" + ATMflds.trnDATE + "," + ATMflds.AMOUNT + ",'" + ATMflds.UTRNNO + "," + ATMflds.SUCCESSFUL + "," + ATMflds.ReasonCode + "," + ATMflds.AtmNo + Environment.NewLine;

                            }
                        }
                    }

                }

                outputFile = outputFolder + "\\Converted_ATMJournal_" + Path.GetFileNameWithoutExtension(inputFile) + "_" + DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff") + ".csv";
                WriteFile(outputFile, scontent);

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
            bool gotResp = false;
            bool gotATM = false;
            bool gotATHNO = false;
            string currentRESP = "";
            string currentDESC = "";
            string currenAMount = "";

            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {//
                if (d[i].Contains("CRD NO:"))
                {
                    if (gotcardno != true)
                    {
                        if (d[i].Length > 24)

                            l.Add("CARDNO:" + d[i].Split(':')[1].Trim() + "|");
                        gotcardno = true;

                    }
                }
                if (d[i].Contains("CRD:"))
                {
                    if (gotcardno != true)
                    {
                        if (d[i].Length > 24)

                            l.Add("CARDNO:" + d[i].Split(':')[1].Trim() + "|");
                        gotcardno = true;

                    }
                }
                //HIGH AMOUNT

                if (d[i].Contains("HIGH AMOUNT"))
                {
                    gotamount = false;
                }
                //CRD:
                if (d[i].Contains("0DN"))
                {
                    if (gotdate != true)
                    {
                        if (d[i].ToString().Length < 27)
                        {
                            l.Add("DATE:" + d[i].Substring(0, 9).Replace(".", "/") + " " + d[i].Substring(9, 5));
                            gotdate = true;
                        }
                        if (gotATM == !true)
                        {
                            l.Add("ATMNO:" + d[i].Split(" ")[2]);
                            gotATM = true;
                        }
                        else
                        {
                            l.Add(d[i].ToString().Replace("0DN02001", "").Trim());
                            gotdate = true;
                        }

                    }

                }

                if (d[i].Contains("DISP:"))
                {
                    if (d[i].Trim() != currenAMount && currenAMount != "")
                    {
                        l.Remove(currenAMount);
                        gotamount = false;
                    }
                    if (gotamount != true)
                    {
                        l.Add("AMOUNT:" + d[i].Replace("DISP:", "").Trim());
                        currenAMount = "AMOUNT:" + d[i].Replace("DISP:", "").Trim();
                        gotamount = true;

                    }
                }

                if (d[i].Contains("COMMUNICATION ERROR"))
                {
                    if (gotRC != true)
                    {
                        l.Add("DESC:" + " COMMUNICATION ERROR");
                        gotRC = true;

                    }
                }
                //TXN:
                if (d[i].Contains("TXN:"))
                {
                    if (d[i] != currentDESC && currentDESC != "")
                    {
                        string sd = "";
                        l.Remove(currentDESC);
                        gotRC = false;
                    }
                    if (gotRC != true)
                    {
                        l.Add("DESC:" + d[i].Trim().Split(":")[1]);
                        currentDESC = "DESC:" + d[i].Trim().Split(":")[1];
                        gotRC = true;

                    }
                }
                if (d[i].Contains("BALANCE ENQUIRY"))
                {
                    if (d[i] != currentDESC && currentDESC != "")
                    {
                        string sd = "";
                        l.Remove(currentDESC);
                        gotRC = false;
                    }
                    if (gotRC != true)
                    {
                        l.Add("DESC:" + d[i].Trim().Split(":")[1]);
                        currentDESC = "DESC:" + d[i].Trim().Split(":")[1];
                        gotRC = true;

                    }
                }
                if (d[i].Contains("DESC:"))
                {
                    if (d[i] != currentDESC && currentDESC != "")
                    {
                        string sd = "";
                        l.Remove(currentDESC);
                        gotRC = false;
                    }
                    if (gotRC != true)
                    {
                        l.Add(d[i].Trim());
                        currentDESC = d[i].Trim();
                        gotRC = true;

                    }
                }
                if (d[i].Contains("RESP:"))
                {
                    if (d[i].Trim() != currentRESP && currentRESP != "")
                    {

                        l.Remove(currentRESP);
                        gotRC = false;
                    }
                    if (gotRC != true)
                    {
                        if (d[i].Split(':')[1].Trim() == "00")
                        {
                            l.Add(d[i].Trim());
                            currentRESP = d[i].Trim();
                            gotRC = true;
                        }
                        if (d[i].Split(':')[1].Trim() == "01")
                        {
                            l.Add(d[i]);
                            currentRESP = d[i].Trim();
                            gotRC = true;
                        }
                        else
                        {
                            l.Add(d[i]);
                            currentRESP = d[i].Trim();
                            gotRC = true;
                        }
                    }

                }
                if (d[i].Contains("RESPONSE CODE:"))
                {
                    if (gotRC != true)
                    {
                        l.Add(d[i].Split("\r\n")[0] + "|");
                        gotRC = true;
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
                if (d[i].Contains("CASH RETRACTED"))
                {
                    if (gotcashtaken != true)
                    {
                        if (d[i].Split("\r\n")[0].Length < 20)
                        {
                            l.Add("DESC:" + d[i].Split("\r\n")[0].Substring(9, 10) + "|");
                            gotcashtaken = true;
                        }
                        else
                        {
                            l.Add("DESC:" + "CASH RETRACTED");
                            gotcashtaken = true;
                        }
                    }

                }
                //REF.NO:
                if (d[i].Contains("REF.NO:"))
                {
                    if (gotSEQ != true)
                    {
                        if (d[i].Length < 40)
                        {
                            l.Add("SEQ:" + d[i].Split(':')[1].Trim() + "|");
                            gotSEQ = true;
                        }
                        else
                        {
                            l.Add("SEQ:" + d[i] + "|");
                            gotSEQ = true;
                        }
                    }

                }
                if (d[i].Contains("REF. NO:"))
                {
                    if (gotSEQ != true)
                    {
                        if (d[i].Length < 40)
                        {
                            l.Add("SEQ:" + d[i].Split(':')[1].Trim() + "|");
                            gotSEQ = true;
                        }
                        else
                        {
                            l.Add("SEQ:" + d[i] + "|");
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
