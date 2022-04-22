using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class OUTMt320Converter
    {
        public void ProcessOutMt320File(string file, string outputFolder = null)
        {

            string content = File.ReadAllText(file);

            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetDirectoryName(file);
            }

            string[] sDet = File.ReadAllLines(file);

            string[] sGrp = content.Split('{');
            string sType = sGrp[2].Substring(3, 3);
            string scontent = "";

            if (sType == "320")
            {
                MandatorySequence320A seq15A = new MandatorySequence320A();
                MandatorySequence320B seq15B = new MandatorySequence320B();
                MandatorySequence320C seq15C = new MandatorySequence320C();
                MandatorySequence320D seq15D = new MandatorySequence320D();
                MandatorySequence320E seq15E = new MandatorySequence320E();


                List<string> l = this.GetRtgsDetails320(sDet);

                seq15A.NewSequenceA = "";
                seq15A.SenderRef15A = l.First(p => p.StartsWith(":20:")).Split(':')[2].ToString();
                seq15A.RelatedRef15A = l.Any(p => p.StartsWith(":21:")) ? l.First(p => p.StartsWith(":21:")).Split(':')[2].ToString() : "";
                seq15A.TypeofOperation15A = l.Any(p => p.StartsWith(":22A:")) ? l.First(p => p.StartsWith(":22A:")).Split(':')[2].ToString() : "";
                seq15A.ScopeofOperation15A = l.Any(p => p.StartsWith(":94A:")) ? l.First(p => p.StartsWith(":94A:")).Split(':')[2].ToString() : "";
                seq15A.TypeofEvent15A = l.Any(p => p.StartsWith(":22B:")) ? l.First(p => p.StartsWith(":22B:")).Split(':')[2].ToString() : "";
                seq15A.CommonReference15A = l.Any(p => p.StartsWith(":22C:")) ? l.First(p => p.StartsWith(":22C:")).Split(':')[2].ToString() : "";
                seq15A.PartyA15A = l.Any(p => p.StartsWith(":82A:")) ? l.First(p => p.StartsWith(":82A:")).Split(':')[2].ToString() : "";
                seq15A.PartyB15A = l.Any(p => p.StartsWith(":87A:")) ? l.First(p => p.StartsWith(":87A:")).Split(':')[2].ToString() : "";

                seq15A.TermsandConditions15A = l.Any(p => p.StartsWith(":77D:")) ? l.First(p => p.StartsWith(":77D:")).Split(':')[2].ToString() : "";

                seq15B.NewSequenceB = "";
                seq15B.PartyAsRole15B = l.Any(p => p.StartsWith(":17R:")) ? l.First(p => p.StartsWith(":17R:")).Split(':')[2].ToString() : "";
                seq15B.TradeDate15B = l.Any(p => p.StartsWith(":30T:")) ? l.First(p => p.StartsWith(":30T:")).Split(':')[2].ToString() : "";
                seq15B.ValueDate15B = l.Any(p => p.StartsWith(":30V:")) ? l.First(p => p.StartsWith(":30V:")).Split(':')[2].ToString() : "";
                seq15B.MaturityDate15B = l.Any(p => p.StartsWith(":30P:")) ? l.First(p => p.StartsWith(":30P:")).Split(':')[2].ToString().Replace(',', '.') : "";
                seq15B.CurrencyPrincipalAmount15B = l.Any(p => p.StartsWith(":32B:")) ? l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Substring(0, 3) : "";
                seq15B.PrincipalAmount15B = l.Any(p => p.StartsWith(":32B:")) ? l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) : l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) + "." + l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.CurrencyAmounttobeSettled15B = l.Any(p => p.StartsWith(":32H:")) ? l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Trim().Substring(0, 3) : "";
                seq15B.AmounttobeSettledt15B = l.Any(p => p.StartsWith(":32H:")) ? l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) : l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) + "." + l.First(p => p.StartsWith(":32H:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.NextInterestDueDate15B = l.Any(p => p.StartsWith(":30X:")) ? l.First(p => p.StartsWith(":30X:")).Split(':')[2].ToString() : "";
                seq15B.CurrencyInterestAmount15B = l.Any(p => p.StartsWith(":34E:")) ? l.First(p => p.StartsWith(":34E:")).Split(':')[2].ToString().Trim().Substring(0, 3) : "";
                seq15B.InterestAmount15B = l.Any(p => p.StartsWith(":32E:")) ? l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) : l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) + "." + l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.InterestRate15B = l.Any(p => p.StartsWith(":37G:")) ? l.First(p => p.StartsWith(":37G:")).Split(':')[2].ToString().Replace(',', '.') : "";
                seq15B.DayCountFraction15B = l.Any(p => p.StartsWith(":14D:")) ? l.First(p => p.StartsWith(":14D:")).Split(':')[2].ToString() : "";
                seq15B.LastDayoftheFirstInterestPeriod15B = l.Any(p => p.StartsWith(":30F:")) ? l.First(p => p.StartsWith(":30F:")).Split(':')[2].ToString() : "";
                seq15B.NumberofDays15B = l.Any(p => p.StartsWith(":38J:")) ? l.First(p => p.StartsWith(":38J:")).Split(':')[2].ToString() : "";


                seq15C.NewSequenceC = "";
                seq15C.ReceivingAgent15C = l.Any(p => p.StartsWith(":57A:")) ? l.First(p => p.StartsWith(":57A:")).Split(':')[2].ToString() : "";

                seq15D.NewSequenceD = "";
                seq15D.DeliveryAgent15D = l.Any(p => p.StartsWith(":53A:")) ? l.First(p => p.StartsWith(":53A:")).Split(':')[2].ToString() : "";
                seq15D.ReceivingAgent15D = l.Any(p => p.StartsWith("T:57A:")) ? l.First(p => p.StartsWith("T:57A:")).Split('|')[1].ToString() : "";
                seq15D.BeneficiaryInstitution15D = l.Any(p => p.StartsWith(":58A:")) ? l.First(p => p.StartsWith(":58A:")).Split('|')[0].ToString() : "";


                scontent = " New Sequence A, Sender's Reference,Related Reference,Type of Operation,Scope of Operation,Type of Event,Common Reference ,  " +
                "Party A - BIC, Party B - BIC,New Sequence B,   " +
                "Party A's Role, Trade Date,Value Date,Maturity Date,Currency,Principal Amount,Currency,Amount to be Settled,Next Interest Due Date,Currency,Interest Amount,Interest Rate,Day Count Fraction,Last Day of First Int Per,Number of Days, " +
                " New Sequence C ,Receiving Agent - FI BIC," +
                " New Sequence D,Delivery Agent - FI BIC,Receiving Agent - FI BIC,Beneficiary Institution - BIC " + Environment.NewLine;
                scontent += seq15A.NewSequenceA + "," + seq15A.SenderRef15A + "," + seq15A.RelatedRef15A + "," + seq15A.TypeofOperation15A + "," + seq15A.ScopeofOperation15A + "," + seq15A.TypeofEvent15A + "," + seq15A.CommonReference15A + "," + seq15A.PartyA15A + "," + seq15A.PartyB15A;
                scontent += "," + seq15B.NewSequenceB + "," + seq15B.PartyAsRole15B + "," + seq15B.TradeDate15B + "," + seq15B.ValueDate15B + "," + seq15B.MaturityDate15B + "," + seq15B.CurrencyPrincipalAmount15B + "," + seq15B.PrincipalAmount15B + "," + seq15B.CurrencyAmounttobeSettled15B + "," + seq15B.AmounttobeSettledt15B + "," + seq15B.NextInterestDueDate15B + "," + seq15B.CurrencyInterestAmount15B + "," + seq15B.InterestAmount15B + "," + seq15B.InterestRate15B + "," + seq15B.DayCountFraction15B + "," + seq15B.LastDayoftheFirstInterestPeriod15B + "," + seq15B.NumberofDays15B;
                scontent += "," + seq15C.NewSequenceC + "," + seq15C.ReceivingAgent15C;
                scontent += "," + seq15D.NewSequenceD + "," + seq15D.DeliveryAgent15D + "," + seq15D.ReceivingAgent15D + "," + seq15D.BeneficiaryInstitution15D;
                WriteFile(outputFolder + "\\Converted_MT320_" + Path.GetFileNameWithoutExtension(file) + ".csv", scontent);
            }

        }
        private List<string> GetRtgsDetails320(string[] d)
        {
            List<string> l = new List<string>();
            for (int i = 1; i < d.Length - 1; i++)
            {
                if (!d[i].StartsWith(":"))
                {
                    if (!d[i - 1].StartsWith(":"))
                    {
                        if (!d[i - 2].StartsWith(":"))
                        {
                            if (!d[i - 3].StartsWith(":"))
                            {
                                d[i - 4] += "|" + d[i];
                                d[i] = "";
                            }
                            else
                            {
                                d[i - 3] += "|" + d[i];
                                d[i] = "";
                            }
                        }
                        else
                        {
                            d[i - 2] += "|" + d[i];
                            d[i] = "";
                        }
                    }
                    else
                    {
                        d[i - 1] += "|" + d[i];
                        d[i] = "";
                    }
                }
            }
            for (int i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("57A:"))
                {
                    if (i < 21)
                    {
                        if (d[i].Trim() != "")
                            l.Add(d[i]);
                    }
                    else
                    {
                        l.Add("T" + d[i]);

                    }
                }
                else
                {
                    if (d[i].Trim() != "")
                        l.Add(d[i]);
                }
            }
            return l;
        }
        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
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

    }


}
