using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class Mt300Converter
    {
        public void ProcessMt300File(string file, string outputFolder = null)
        {
            var content = File.ReadAllText(file);

            if (string.IsNullOrEmpty(outputFolder)) outputFolder = Path.GetDirectoryName(file);

            var sDet = File.ReadAllLines(file);
            var scontent = "";
            var sType = content.Split(':')[6].Trim().Substring(4, 3);
            if (content.Split(':')[6].Trim().Substring(0, 7) != "FIN 300")
            {
                var archive = "";

                archive = Path.Combine(Path.GetDirectoryName(file) + "\\MT300", "FAILED",
                    DateTime.Now.ToString("yyMMdd") + "\\RTGSMT300");
                if (!Directory.Exists(archive))
                    Directory.CreateDirectory(archive);


                try
                {
                    if (sType == "320")
                    {
                        File.Copy(file, archive + "\\320_" + Path.GetFileName(file));
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
                var seq15A = new MandatorySequenceA();
                var seq15B = new MandatorySequenceB();
                var seq15C = new MandatorySequenceC();
                var seq15E = new MandatorySequenceE();


                var l = GetRtgsDetails_MT300_15A(sDet);
                var lb = GetRtgsDetails_MT300_15B(sDet);
                var lc = GetRtgsDetails_MT300_15C(sDet);
                var le = GetRtgsDetails_MT300_15E(sDet);

                seq15A.NewSequenceA = "";
                seq15A.SenderRef15A = l.First(p => p.StartsWith("20:")).Split('|')[1].ToString();
                seq15A.RelatedRef15A = l.Any(p => p.StartsWith("21:"))
                    ? l.First(p => p.StartsWith("21:")).Substring(4).Replace("|", "")
                    : "";
                seq15A.TypeofOperation15A = l.Any(p => p.StartsWith("22A:"))
                    ? l.First(p => p.StartsWith("22A:")).Split('|')[1].ToString()
                    : "";
                seq15A.ScopeofOperation15A = l.Any(p => p.StartsWith("94A:"))
                    ? l.First(p => p.StartsWith("94A:")).Split('|')[1].ToString()
                    : "";
                seq15A.CommonReference15A = l.Any(p => p.StartsWith("22C:"))
                    ? l.First(p => p.StartsWith("22C:")).Split('|')[1].ToString()
                    : "";
                seq15A.BlockTradeIndicator15A = l.Any(p => p.StartsWith("17T:"))
                    ? l.First(p => p.StartsWith("17T:")).Split('|')[1].ToString()
                    : "";
                seq15A.SplitSettlementIndicator15A = l.Any(p => p.StartsWith("17U:"))
                    ? l.First(p => p.StartsWith("17U:")).Split('|')[1].ToString()
                    : "";
                seq15A.PaymentversusPaymentSettlementIndicator15A = l.Any(p => p.StartsWith("17I:"))
                    ? l.First(p => p.StartsWith("17I:")).Split('|')[1].ToString()
                    : "";
                seq15A.PartyA15A = l.Any(p => p.StartsWith("82A:"))
                    ? l.First(p => p.StartsWith("82A:")).Split('|')[1].ToString()
                    : "";
                seq15A.PartyB15A = l.Any(p => p.StartsWith("87A:"))
                    ? l.First(p => p.StartsWith("87A:")).Split('|')[1].ToString()
                    : "";
                seq15A.TypeDateVersionoftheAgreement15A = l.Any(p => p.StartsWith("77H:"))
                    ? l.First(p => p.StartsWith("77H:")).Split('|')[1].ToString()
                    : "";
                seq15A.TermsandConditions15A = l.Any(p => p.StartsWith("77D:"))
                    ? l.First(p => p.StartsWith("77D:")).Split('|')[1].ToString()
                    : "";
                seq15A.YearofDefinitions15A = l.Any(p => p.StartsWith("14C:"))
                    ? l.First(p => p.StartsWith("14C:")).Split('|')[1].ToString()
                    : "";
                seq15A.NonDeliverableIndicator15A = l.Any(p => p.StartsWith("17F:"))
                    ? l.First(p => p.StartsWith("17F:")).Split('|')[1].ToString()
                    : "";
                seq15A.NDFOpenIndicator15A = l.Any(p => p.StartsWith("17O:"))
                    ? l.First(p => p.StartsWith("17O:")).Split('|')[1].ToString()
                    : "";
                seq15A.SettlementCurrency15A = l.Any(p => p.StartsWith("32E:"))
                    ? l.First(p => p.StartsWith("32E:")).Split('|')[1].ToString().Split(':')[1].Trim().Substring(0, 3)
                    : "";
                seq15A.ValuationDate15A = l.Any(p => p.StartsWith("30U:"))
                    ? l.First(p => p.StartsWith("30U:")).Split('|')[1].ToString()
                    : "";
                seq15A.SettlementRateSource15A = l.Any(p => p.StartsWith("14S:"))
                    ? l.First(p => p.StartsWith("14S:")).Split('|')[1].ToString()
                    : "";
                seq15A.ReferencetoOpeningConfirmation5A = l.Any(p => p.StartsWith("21A:"))
                    ? l.First(p => p.StartsWith("21A:")).Split('|')[1].ToString()
                    : "";
                seq15A.ClearingorSettlementSession5A = l.Any(p => p.StartsWith("14E:"))
                    ? l.First(p => p.StartsWith("14E:")).Split('|')[1].ToString()
                    : "";


                seq15B.NewSequenceB = "";
                seq15B.TradeDate15B = lb.Any(p => p.StartsWith("30T:"))
                    ? lb.First(p => p.StartsWith("30T:")).Split('|')[1].ToString()
                    : "";
                seq15B.ValueDate15B = lb.Any(p => p.StartsWith("30V:"))
                    ? lb.First(p => p.StartsWith("30V:")).Split('|')[1].ToString()
                    : "";
                seq15B.ExchangeRate15B = lb.Any(p => p.StartsWith("36:"))
                    ? lb.First(p => p.StartsWith("36:")).Split('|')[1].ToString().Replace(',', '.')
                    : "";
                seq15B.CurrencyAmountbought15B = lb.Any(p => p.StartsWith("32B:"))
                    ? lb.First(p => p.StartsWith("32B:")).Split('|')[1].ToString().Split(':')[1].Trim().Substring(0, 3)
                    : "";
                seq15B.Amountbought15B = lb.Any(p => p.StartsWith("32B:"))
                    ? lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Trim().Split(',')[1]
                        .Replace("#", "") == ""
                        ?
                        lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0]
                            .Replace("#", "")
                        : lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')
                              [0].Replace("#", "") + "." +
                          lb.First(p => p.StartsWith("32B:")).Split('|')[2].ToString().Split(':')[1].Trim()
                              .Split(',')[1]
                              .Replace("#", "")
                    : "";
                seq15B.IntermediaryAmountbought15B = lb.Any(p => p.StartsWith("56A:"))
                    ? lb.First(p => p.StartsWith("56A:")).Split('|')[1].ToString()
                    : "";
                seq15B.ReceivingAgentAmountbought15B = lb.Any(p => p.StartsWith("57A:"))
                    ? lb.First(p => p.StartsWith("57A:")).Split('|')[1].ToString()
                    : "";
                seq15B.CurrencyAmountSold15B = lb.Any(p => p.StartsWith("33B:"))
                    ? lb.First(p => p.StartsWith("33B:")).Split('|')[1].ToString().Split(':')[1].Trim().Substring(0, 3)
                    : "";
                seq15B.AmountSold15B = lb.Any(p => p.StartsWith("33B:"))
                    ? lb.First(p => p.StartsWith("33B:")).Split('|')[2].ToString().Trim().Split(',')[1]
                        .Replace("#", "") == ""
                        ?
                        lb.First(p => p.StartsWith("33B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')[0]
                            .Replace("#", "")
                        : lb.First(p => p.StartsWith("33B:")).Split('|')[2].ToString().Split(':')[1].Trim().Split(',')
                              [0].Replace("#", "") + "." +
                          lb.First(p => p.StartsWith("33B:")).Split('|')[2].ToString().Split(':')[1].Trim()
                              .Split(',')[1]
                              .Replace("#", "")
                    : "";
                seq15B.DeliveryAgentAmountSold15B = lb.Any(p => p.StartsWith("53A:"))
                    ? lb.First(p => p.StartsWith("53A:")).Split('|')[1].ToString()
                    : "";
                seq15B.IntermediaryAmountSold15B = lb.Any(p => p.StartsWith("56A:"))
                    ? lb.First(p => p.StartsWith("56A:")).Split('~')[1].ToString()
                    : "";
                seq15B.ReceivingAgentAmountSold15B = lb.Any(p => p.StartsWith("T57A:"))
                    ? lb.First(p => p.StartsWith("T57A:")).Split('~')[1].ToString()
                    : "";
                seq15B.BeneficiaryInstitutionAmountSold15B = lb.Any(p => p.StartsWith("58A:"))
                    ? lb.First(p => p.StartsWith("58A:")).Split('|')[1].ToString()
                    : "";


                seq15C.NewSequenceC = "";
                seq15C.DealingMethod15C = lc.Any(p => p.StartsWith("24D:"))
                    ? lc.First(p => p.StartsWith("24D:")).Split('|')[1].ToString()
                    : "";

                seq15E.NewSequenceE = "";
                seq15E.ExecutionVenue15E = le.Any(p => p.StartsWith("22V:"))
                    ? le.First(p => p.StartsWith("22V:")).Split('|')[1].ToString()
                    : "";
                seq15E.ExecutionTimestamp15E = le.Any(p => p.StartsWith("98D:"))
                    ? le.First(p => p.StartsWith("98D:")).Split('|')[1].ToString()
                    : "";

                scontent =
                    " New Sequence A, Sender's Reference,Type of Operation,Scope of Operation,Common Reference ,  " +
                    "Split Settlement Indicator,Party A - BIC, Party B - BIC, Non - Deliverable Indicator,New Sequence B,   " +
                    " Trade Date,Value Date,Exchange Rate,Currency,Amount,Receiving Agent - FI BIC,Currency,Amount, " +
                    " Delivery Agent - FI BIC ,Receiving Agent - FI BIC," +
                    " New Sequence C,Dealing Method,New Sequence E,Execution Venue,Execution Timestamp" +
                    Environment.NewLine;
                scontent += seq15A.NewSequenceA + "," + seq15A.SenderRef15A + "," + seq15A.TypeofOperation15A + "," +
                            seq15A.ScopeofOperation15A + "," + seq15A.CommonReference15A + "," +
                            seq15A.SplitSettlementIndicator15A + "," + seq15A.PartyA15A + "," + seq15A.PartyB15A + "," +
                            seq15A.NonDeliverableIndicator15A;
                scontent += "," + seq15B.NewSequenceB + "," + seq15B.TradeDate15B + "," + seq15B.ValueDate15B + "," +
                            seq15B.ExchangeRate15B + "," + seq15B.CurrencyAmountbought15B;
                scontent += "," + seq15B.Amountbought15B + "," + seq15B.ReceivingAgentAmountbought15B + "," +
                            seq15B.CurrencyAmountSold15B + "," + seq15B.AmountSold15B + "," +
                            seq15B.DeliveryAgentAmountSold15B + "," + seq15B.ReceivingAgentAmountSold15B;
                scontent += "," + seq15C.NewSequenceC + "," + seq15C.DealingMethod15C + "," + seq15E.NewSequenceE +
                            "," + seq15E.ExecutionVenue15E + "," + seq15E.ExecutionTimestamp15E;
                WriteFile(outputFolder + "\\Converted_MT300_" + Path.GetFileNameWithoutExtension(file) + ".csv",
                    scontent); // DateTime.Now.ToString("yyyy_MM_dd_HHmmssfff")
            }
        }

        public static void WriteFile(string path, string content)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(content);
                }
            }
        }


        private List<string> GetRtgsDetails_MT300_15A(string[] d)
        {
            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("15A:")) l.Add(d[i].Trim());
                if (d[i].Contains("20:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                //if (d[i].Contains("21:"))
                //{
                //    l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                //}
                if (d[i].Contains("22A:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("94A:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("22C:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("22B:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("22C:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("17U:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("82A:"))
                {
                    if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                        l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                    else
                        l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                }

                if (d[i].Contains("87A:"))
                {
                    if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                        l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                    else
                        l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                }

                if (d[i].Contains("17F:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
            }

            return l;
        }

        private List<string> GetRtgsDetails_MT300_15B(string[] d)
        {
            var fldcount = 0;
            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("15B:")) l.Add(d[i].Trim());

                if (d[i].Contains("30T:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("30V:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                if (d[i].Contains("36:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("32B:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim() + "|" + d[i + 2].Trim());
                if (d[i].Contains("33B:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim() + "|" + d[i + 2].Trim());
                if (d[i].Contains("24D:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("22V:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                if (d[i].Contains("98D:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                if (d[i].Contains("15C:")) l.Add(d[i].Trim());
                if (d[i].Contains("57A:"))
                {
                    if (i < 60)
                    {
                        if (d[i + 2].Trim().Length != 8 || d[i + 2].Trim().Length != 11)
                            l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                        else
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                    }
                    else
                    {
                        if (d[i + 2].Trim().Length != 8 || d[i + 2].Trim().Length != 11)
                            //if (d[i + 1].Trim().Length != 8 || d[i + 2].Trim().Length != 8)
                            //{
                            l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                        //}
                        //else
                        //{ l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim()); }
                        else
                            l.Add("T" + d[i].Trim() + "~" + d[i + 3].Trim());
                    }
                }

                if (d[i].Contains("53A:"))
                {
                    if (i < 78)
                    {
                        if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                            l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
                        else
                            l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                    }
                    else
                    {
                        if (d[i + 1].Trim().Length != 8 || d[i + 1].Trim().Length != 11)
                            l.Add("T" + d[i].Trim() + "~" + d[i + 1].Trim());
                        else
                            l.Add("T" + d[i].Trim() + "~" + d[i + 2].Trim());
                    }
                }

                if (d[i].Contains("58A:")) l.Add(d[i].Trim() + "|" + d[i + 2].Trim());
                fldcount++;
            }

            return l;
        }

        private List<string> GetRtgsDetails_MT300_15C(string[] d)
        {
            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("15C:")) l.Add(d[i].Trim());
                if (d[i].Contains("24D:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
            }

            return l;
        }

        private List<string> GetRtgsDetails_MT300_15E(string[] d)
        {
            var l = new List<string>();
            for (var i = 1; i < d.Length - 1; i++)
            {
                if (d[i].Contains("15E:")) l.Add(d[i].Trim());
                if (d[i].Contains("22V:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());

                if (d[i].Contains("98D:")) l.Add(d[i].Trim() + "|" + d[i + 1].Trim());
            }

            return l;
        }
    }

    public class MandatorySequenceA
    {
        public string NewSequenceA { get; set; }
        public string SenderRef15A { get; set; }
        public string RelatedRef15A { get; set; }
        public string TypeofOperation15A { get; set; }
        public string ScopeofOperation15A { get; set; }
        public string CommonReference15A { get; set; }
        public string SplitSettlementIndicator15A { get; set; }
        public string BlockTradeIndicator15A { get; set; }
        public string PaymentversusPaymentSettlementIndicator15A { get; set; }
        public string PartyA15A { get; set; }
        public string PartyB15A { get; set; }
        public string FundorBeneficiaryCustomer15A { get; set; }
        public string TypeDateVersionoftheAgreement15A { get; set; }
        public string TermsandConditions15A { get; set; }
        public string YearofDefinitions15A { get; set; }
        public string NonDeliverableIndicator15A { get; set; }
        public string NDFOpenIndicator15A { get; set; }
        public string SettlementCurrency15A { get; set; }
        public string ValuationDate15A { get; set; }
        public string SettlementRateSource15A { get; set; }
        public string ReferencetoOpeningConfirmation5A { get; set; }

        public string ClearingorSettlementSession5A { get; set; }
    }

    public class MandatorySequenceB
    {
        public string NewSequenceB { get; set; }
        public string TradeDate15B { get; set; }
        public string ValueDate15B { get; set; }
        public string ExchangeRate15B { get; set; }
        public string PaymentClearingCentre15B { get; set; }
        public string CurrencyAmountbought15B { get; set; }
        public string Amountbought15B { get; set; }
        public string DeliveryAgentAmountbought15B { get; set; }
        public string IntermediaryAmountbought15B { get; set; }
        public string ReceivingAgentAmountbought15B { get; set; }
        public string CurrencyAmountSold15B { get; set; }
        public string AmountSold15B { get; set; }
        public string DeliveryAgentAmountSold15B { get; set; }
        public string IntermediaryAmountSold15B { get; set; }
        public string ReceivingAgentAmountSold15B { get; set; }
        public string BeneficiaryInstitutionAmountSold15B { get; set; }
    }

    public class MandatorySequenceC
    {
        public string NewSequenceC { get; set; }
        public string ContactInformation15C { get; set; }
        public string DealingMethod15C { get; set; }
        public string DealingBranchPartyA15C { get; set; }
        public string DealingBranchPartyB15C { get; set; }
        public string BrokerIdentification15C { get; set; }
        public string BrokersCommission15C { get; set; }
        public string CounterpartysReference15C { get; set; }
        public string BrokersReference15C { get; set; }
        public string SendertoReceiverInformation15C { get; set; }
    }

    public class MandatorySequenceE
    {
        public string NewSequenceE { get; set; }
        public string ReportingJurisdiction15E { get; set; }
        public string ReportingParty15E { get; set; }

        public string ExecutionVenue15E { get; set; }

        public string ExecutionTimestamp15E { get; set; }
    }
}