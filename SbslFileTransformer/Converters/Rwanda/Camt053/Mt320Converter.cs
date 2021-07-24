using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class Mt320Converter
    {

        public void ProcessMt320File(string file, string outputFolder = null)
        {
            var content = File.ReadAllText(file);

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(file);
            }

            string[] sDet = File.ReadAllLines(file);
            string scontent = "";
            string sType = content.Split(':')[6].ToString().Trim().Substring(4, 3);
            if (content.Split(':')[6].ToString().Trim().Substring(0, 7) != "FIN 320")
            {
                string archive = "";

                archive = Path.Combine(Path.GetDirectoryName(file) + "\\MT320", "FAILED", DateTime.Now.ToString("yyMMdd") + "\\RTGSMT300");
                if (!Directory.Exists(archive))
                    Directory.CreateDirectory(archive);


                try
                {
                    if (sType == "300")
                    {
                        File.Copy(file, archive + "\\300_" + Path.GetFileName(file));
                        File.Delete(file);
                    }
                    else
                    {
                        File.Copy(file, archive + "\\" + Path.GetFileNameWithoutExtension(file) + ".out");
                        File.Delete(file);
                    }


                }
                catch (Exception xc)
                {

                }

                return;


            }


            if (sDet.Length != 0)
            {
                var seq15A = new MandatorySequence320A();
                var seq15B = new MandatorySequence320B();
                var seq15C = new MandatorySequence320C();
                var seq15D = new MandatorySequence320D();
                var seq15E = new MandatorySequence320E();



                List<string> l = GetRtgsDetails_MT320_15A(sDet);
                List<string> lb = GetRtgsDetails_MT320_15B(sDet);
                List<string> lc = GetRtgsDetails_MT320_15C(sDet);
                List<string> ld = GetRtgsDetails_MT320_15D(sDet);


                seq15A.NewSequenceA = "";
                seq15A.SenderRef15A = l.First(p => p.StartsWith("20:")).Split('|')[1].ToString();
                try { seq15A.RelatedRef15A = l.Any(p => p.StartsWith("21:")) ? l.First(p => p.StartsWith("21:")).Split('|')[1].ToString() : ""; } catch (Exception ex) { }

                seq15A.TypeofOperation15A = l.Any(p => p.StartsWith("22A:")) ? l.First(p => p.StartsWith("22A:")).Split('|')[1].ToString() : "";
                seq15A.ScopeofOperation15A = l.Any(p => p.StartsWith("94A:")) ? l.First(p => p.StartsWith("94A:")).Split('|')[1].ToString() : "";
                seq15A.TypeofEvent15A = l.Any(p => p.StartsWith("22B:")) ? l.First(p => p.StartsWith("22B:")).Split('|')[1].ToString() : "";
                seq15A.CommonReference15A = l.Any(p => p.StartsWith("22C:")) ? l.First(p => p.StartsWith("22C:")).Split('|')[1].ToString() : "";
                seq15A.PartyA15A = l.Any(p => p.StartsWith("82A:")) ? l.First(p => p.StartsWith("82A:")).Split('|')[1].ToString() : "";
                seq15A.PartyB15A = l.Any(p => p.StartsWith("87A:")) ? l.First(p => p.StartsWith("87A:")).Split('|')[1].ToString() : "";

                seq15A.TermsandConditions15A = l.Any(p => p.StartsWith("77D:")) ? l.First(p => p.StartsWith("77D:")).Split('|')[1].ToString() : "";

                seq15B.NewSequenceB = "";
                seq15B.PartyAsRole15B = lb.Any(p => p.StartsWith("17R:")) ? lb.First(p => p.StartsWith("17R:")).Split('|')[1].ToString() : "";
                seq15B.TradeDate15B = lb.Any(p => p.StartsWith("30T:")) ? lb.First(p => p.StartsWith("30T:")).Split('|')[1].ToString() : "";
                seq15B.ValueDate15B = lb.Any(p => p.StartsWith("30V:")) ? lb.First(p => p.StartsWith("30V:")).Split('|')[1].ToString() : "";
                seq15B.MaturityDate15B = lb.Any(p => p.StartsWith("30P:")) ? lb.First(p => p.StartsWith("30P:")).Split('|')[1].ToString().Replace(',', '.') : "";
                seq15B.CurrencyPrincipalAmount15B = lb.Any(p => p.StartsWith("32B:")) ? lb.First(p => p.StartsWith("32B:")).Split('|')[1].ToString().Split(':')[1].Trim().Substring(0, 3) : "";
                seq15B.PrincipalAmount15B = lb.Any(p => p.StartsWith("32B:")) ? lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0].Replace("#", "") : lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0].Replace("#", "") + "." + lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.CurrencyAmounttobeSettled15B = lb.Any(p => p.StartsWith("32H:")) ? lb.First(p => p.StartsWith("32H:")).Split('|')[1].ToString().Split(':')[1].Trim().Substring(0, 3) : "";
                seq15B.AmounttobeSettledt15B = lb.Any(p => p.StartsWith("32H:")) ? lb.First(p => p.StartsWith("32H:")).Split('|')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? lb.First(p => p.StartsWith("32H:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0].Replace("#", "") : lb.First(p => p.StartsWith("32H:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0].Replace("#", "") + "." + lb.First(p => p.StartsWith("32H:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.NextInterestDueDate15B = lb.Any(p => p.StartsWith("30X:")) ? lb.First(p => p.StartsWith("30X:")).Split('|')[1].ToString() : "";
                seq15B.CurrencyInterestAmount15B = lb.Any(p => p.StartsWith("34E:")) ? lb.First(p => p.StartsWith("34E:")).Split('|')[1].ToString().Split(':')[1].Trim().Substring(0, 3) : "";
                seq15B.InterestAmount15B = lb.Any(p => p.StartsWith("34E:")) ? lb.First(p => p.StartsWith("34E:")).Split('|')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? lb.First(p => p.StartsWith("34E:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0].Replace("#", "") : lb.First(p => p.StartsWith("34E:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0].Replace("#", "") + "." + lb.First(p => p.StartsWith("34E:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.InterestRate15B = lb.Any(p => p.StartsWith("37G:")) ? lb.First(p => p.StartsWith("37G:")).Split('|')[1].ToString().Replace(',', '.') : "";
                seq15B.DayCountFraction15B = lb.Any(p => p.StartsWith("14D:")) ? lb.First(p => p.StartsWith("14D:")).Split('|')[1].ToString() : "";
                seq15B.LastDayoftheFirstInterestPeriod15B = lb.Any(p => p.StartsWith("30F:")) ? lb.First(p => p.StartsWith("30F:")).Split('|')[1].ToString() : "";
                seq15B.NumberofDays15B = lb.Any(p => p.StartsWith("38J:")) ? lb.First(p => p.StartsWith("38J:")).Split('|')[1].ToString() : "";


                seq15C.NewSequenceC = "";
                seq15C.ReceivingAgent15C = lc.Any(p => p.StartsWith("57A:")) ? lc.First(p => p.StartsWith("57A:")).Split('|')[1].ToString() : "";
                seq15C.DeliveryAgent15C = lc.Any(p => p.StartsWith("58A:")) ? lc.First(p => p.StartsWith("58A:")).Split('|')[1].ToString() : "";

                seq15D.NewSequenceD = "";
                seq15D.DeliveryAgent15D = ld.Any(p => p.StartsWith("53A:")) ? ld.First(p => p.StartsWith("53A:")).Split('|')[1].ToString() : "";
                seq15D.ReceivingAgent15D = ld.Any(p => p.StartsWith("T57A:")) ? ld.First(p => p.StartsWith("T57A:")).Split('~')[1].ToString() : "";
                seq15D.BeneficiaryInstitution15D = ld.Any(p => p.StartsWith("T58A:")) ? ld.First(p => p.StartsWith("T58A:")).Split('~')[1].ToString() : "";

                scontent = " New Sequence A,Sender Reference,Type of operation,Scope Of Operation,Type Of Event,Common Reference,Contract NO,Party A - BIC,Party B - BIC,  " +
                             "New Sequence B,Partys A role,Trade date,Value date,Maturity Date,Currency,Principal Amount,Interest Date,Currency,Interest Amount,Interest Rate,Day Count, " +
                             "New Sequence C,Delivery Agent 1,Receiving Agent 1, " +
                            " New Sequence D, Delivery Agent 2, Receiving Agent 2  " + Environment.NewLine;
                scontent += seq15A.NewSequenceA + "," + seq15A.SenderRef15A + "," + seq15A.TypeofOperation15A + "," + seq15A.ScopeofOperation15A + "," + seq15A.TypeofEvent15A + "," + seq15A.CommonReference15A + "," + seq15A.ContractNumberPartyA15A + "," + seq15A.PartyA15A + "," + seq15A.PartyB15A;
                scontent += "," + seq15B.NewSequenceB + "," + seq15B.PartyAsRole15B + "," + seq15B.TradeDate15B + "," + seq15B.ValueDate15B + "," + seq15B.MaturityDate15B + "," + seq15B.CurrencyPrincipalAmount15B + "," + seq15B.PrincipalAmount15B + "," + seq15B.NextInterestDueDate15B + "," + seq15B.CurrencyInterestAmount15B + "," + seq15B.InterestAmount15B + "," + seq15B.InterestRate15B + "," + seq15B.DayCountFraction15B;
                scontent += "," + seq15C.NewSequenceC + "," + seq15C.DeliveryAgent15C + "," + seq15C.ReceivingAgent15C;
                scontent += "," + seq15D.NewSequenceD + "," + seq15D.DeliveryAgent15D + "," + seq15D.ReceivingAgent15D;

                WriteFile(outputFolder + "\\Converted_MT320_" + Path.GetFileNameWithoutExtension(file) + ".csv", scontent);
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

        public class MandatorySequence320A
        {
            public string NewSequenceA { get; set; }
            public string SenderRef15A { get; set; }
            public string RelatedRef15A { get; set; }
            public string TypeofOperation15A { get; set; }
            public string ScopeofOperation15A { get; set; }
            public string CommonReference15A { get; set; }
            public string TypeofEvent15A { get; set; }
            public string ContractNumberPartyA15A { get; set; }

            public string PartyA15A { get; set; }
            public string PartyB15A { get; set; }
            public string FundorInstructingParty15A { get; set; }
            public string TermsandConditions15A { get; set; }

        }

        public class MandatorySequence320B
        {
            public string NewSequenceB { get; set; }

            public string PartyAsRole15B { get; set; }

            public string TradeDate15B { get; set; }
            public string ValueDate15B { get; set; }
            public string MaturityDate15B { get; set; }
            //public string PaymentClearingCentre15B { get; set; }
            public string CurrencyPrincipalAmount15B { get; set; }
            public string PrincipalAmount15B { get; set; }
            public string CurrencyAmounttobeSettled15B { get; set; }
            public string AmounttobeSettledt15B { get; set; }
            public string NextInterestDueDate15B { get; set; }
            public string CurrencyInterestAmount15B { get; set; }
            public string InterestAmount15B { get; set; }
            public string DayCountFraction15B { get; set; }
            public string InterestRate15B { get; set; }
            public string LastDayoftheFirstInterestPeriod15B { get; set; }
            public string NumberofDays15B { get; set; }
            public string PaymentClearingCentre15B { get; set; }

        }
        public class MandatorySequence320C
        {
            public string NewSequenceC { get; set; }
            public string DeliveryAgent15C { get; set; }
            public string Intermediary215C { get; set; }
            public string Intermediary15C { get; set; }
            public string ReceivingAgent15C { get; set; }
            public string BeneficiaryInstitution15C { get; set; }
            public string BrokersReference15C { get; set; }
            public string SendertoReceiverInformation15C { get; set; }


        }

        public class MandatorySequence320D
        {
            public string NewSequenceD { get; set; }
            public string DeliveryAgent15D { get; set; }
            public string Intermediary215D { get; set; }
            public string Intermediary15D { get; set; }
            public string ReceivingAgent15D { get; set; }
            public string BeneficiaryInstitution15D { get; set; }



        }

        public class MandatorySequence320E
        {
            public string NewSequenceE { get; set; }
            public string DeliveryAgent15E { get; set; }
            public string Intermediary215E { get; set; }

            public string Intermediary15E { get; set; }

            public string ReceivingAgent15E { get; set; }

            public string BeneficiaryInstitution15E { get; set; }

        }

        private List<string> GetRtgsDetails_MT320_15A(string[] d)
        {


            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("15A:"))
                {
                    l.Add(d[i].Trim());


                }
                if (d[i].Contains("20:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("21:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("22A:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("94A:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("22C:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("22B:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("22C:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }

                if (d[i].Contains("82A:"))
                {
                    if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                    {
                        l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                    }
                    else
                    {
                        l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                    }


                }
                if (d[i].Contains("87A:"))
                {
                    if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                    {
                        l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                    }
                    else
                    {
                        l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                    }

                }


            }

            return l;
        }

        private List<string> GetRtgsDetails_MT320_15B(string[] d)
        {


            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("15B:"))
                {
                    l.Add(d[i].Trim());


                }
                if (d[i].Contains("17R:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("30T:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("30V:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }

                if (d[i].Contains("30P:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("32B:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim() + "|" + d[i + 2].Trim());

                }
                if (d[i].Contains("33B:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim() + "|" + d[i + 2].Trim());

                }
                if (d[i].Contains("32H:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim() + "|" + d[i + 2].Trim());

                }
                if (d[i].Contains("30X:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("34E:"))
                {
                    try
                    {
                        if (d[i + 1].Trim().Split(':')[1].Trim().Substring(0, 3) == "USD" || d[i + 1].Trim().Split(':')[1].Trim().Substring(0, 3) == "RWF" || d[i + 1].Trim().Split(':')[1].Trim().Substring(0, 3) == "GBP" || d[i + 1].Trim().Split(':')[1].Trim().Substring(0, 3) == "KES")
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim() + "|" + d[i + 2].Trim());
                        }
                        else
                        {
                            if (d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "USD" || d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "RWF" || d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "GBP" || d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "KES")
                            {
                                l.Add(d[i].Trim() + "|" + d[i + 2].Trim() + "|" + d[i + 3].Trim());
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        if (d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "USD" || d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "RWF" || d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "GBP" || d[i + 2].Trim().Split(':')[1].Trim().Substring(0, 3) == "KES")
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 2].Trim() + "|" + d[i + 3].Trim());
                        }


                    }
                }
                if (d[i].Contains("37G:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("14D:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
                if (d[i].Contains("30F:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }

                if (d[i].Contains("38J:"))
                {
                    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                }
            }

            return l;
        }

        private List<string> GetRtgsDetails_MT320_15C(string[] d)
        {


            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {

                if (d[i].Contains("15C:"))
                {
                    l.Add(d[i].Trim());

                }
                if (d[i].Contains("57A:"))
                {
                    if (i < 71)
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                            }

                        }
                        else
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                        }

                    }
                    else
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                            }
                            //else
                            //{ l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim()); }
                        }
                        else
                        {
                            l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim());
                        }

                    }

                }

                if (d[i].Contains("58A:"))
                {
                    if (i < 71)
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                            }

                        }
                        else
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                        }

                    }
                    else
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                            }
                            //else
                            //{ l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim()); }
                        }
                        else
                        {
                            l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                        }

                    }

                }


            }

            return l;
        }

        private List<string> GetRtgsDetails_MT320_15D(string[] d)
        {


            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {

                if (d[i].Contains("15D:"))
                {
                    l.Add(d[i].Trim());

                }

                if (d[i].Contains("53A:"))
                {
                    if (i < 77)
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                            }

                        }
                        else
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                        }

                    }
                    else
                    {
                        if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                        {

                            l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());

                        }
                        else
                        {
                            l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim());
                        }

                    }

                }
                if (d[i].Contains("57A:"))
                {
                    if (i < 74)
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                            }

                        }
                        else
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                        }

                    }
                    else
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                            }
                        }
                        else
                        {
                            l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim());
                        }

                    }

                }

                if (d[i].Contains("58A:"))
                {
                    if (i < 67)
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                            }

                        }
                        else
                        {
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                        }

                    }
                    else
                    {
                        if (d[i + 1].Trim().Length != 8)
                        {
                            if (d[i + 1].Trim().Length != 11)
                            {
                                l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                            }
                        }
                        else
                        {
                            l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim());
                        }

                    }

                }
            }

            return l;
        }









    }
}