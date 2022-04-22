using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class OUTMt300Converter
    {
        public void ProcessOutMt300File(string file, string outputFolder = null)
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
            if (sType == "300")
            {
                List<string> l = this.GetRtgsDetails(sDet);

                MandatorySequenceA seq15A = new MandatorySequenceA();
                MandatorySequenceB seq15B = new MandatorySequenceB();
                MandatorySequenceC seq15C = new MandatorySequenceC();
                MandatorySequenceE seq15E = new MandatorySequenceE();


                seq15A.NewSequenceA = "";
                seq15A.SenderRef15A = l.First(p => p.StartsWith(":20:")).Split(':')[2].ToString();
                seq15A.RelatedRef15A = l.Any(p => p.StartsWith(":21:")) ? l.First(p => p.StartsWith(":21:")).Substring(4).Replace(":", "") : "";
                seq15A.TypeofOperation15A = l.Any(p => p.StartsWith(":22A:")) ? l.First(p => p.StartsWith(":22A:")).Split(':')[2].ToString() : "";
                seq15A.ScopeofOperation15A = l.Any(p => p.StartsWith(":94A:")) ? l.First(p => p.StartsWith(":94A:")).Split(':')[2].ToString() : "";
                seq15A.CommonReference15A = l.Any(p => p.StartsWith(":22C:")) ? l.First(p => p.StartsWith(":22C:")).Split(':')[2].ToString() : "";
                seq15A.BlockTradeIndicator15A = l.Any(p => p.StartsWith(":17T:")) ? l.First(p => p.StartsWith(":17T:")).Split(':')[2].ToString() : "";
                seq15A.SplitSettlementIndicator15A = l.Any(p => p.StartsWith(":17U:")) ? l.First(p => p.StartsWith(":17U:")).Split(':')[2].ToString() : "";
                seq15A.PaymentversusPaymentSettlementIndicator15A = l.Any(p => p.StartsWith(":17I:")) ? l.First(p => p.StartsWith(":17I:")).Split(':')[2].ToString() : "";
                seq15A.PartyA15A = l.Any(p => p.StartsWith(":82A:")) ? l.First(p => p.StartsWith(":82A:")).Split(':')[2].ToString() : "";
                seq15A.PartyB15A = l.Any(p => p.StartsWith(":87A:")) ? l.First(p => p.StartsWith(":87A:")).Split(':')[2].ToString() : "";
                seq15A.TypeDateVersionoftheAgreement15A = l.Any(p => p.StartsWith(":77H:")) ? l.First(p => p.StartsWith(":77H:")).Split(':')[2].ToString() : "";
                seq15A.TermsandConditions15A = l.Any(p => p.StartsWith(":77D:")) ? l.First(p => p.StartsWith(":77D:")).Split(':')[2].ToString() : "";
                seq15A.YearofDefinitions15A = l.Any(p => p.StartsWith(":14C:")) ? l.First(p => p.StartsWith(":14C:")).Split(':')[2].ToString() : "";
                seq15A.NonDeliverableIndicator15A = l.Any(p => p.StartsWith("17F:")) ? l.First(p => p.StartsWith(":17F:")).Split('|')[2].ToString() : "";
                seq15A.NDFOpenIndicator15A = l.Any(p => p.StartsWith(":17O:")) ? l.First(p => p.StartsWith(":17O:")).Split(':')[2].ToString() : "";
                seq15A.SettlementCurrency15A = l.Any(p => p.StartsWith(":32E:")) ? l.First(p => p.StartsWith(":32E:")).Split(':')[2].ToString().Trim().Substring(0, 3) : "";
                seq15A.ValuationDate15A = l.Any(p => p.StartsWith(":30U:")) ? l.First(p => p.StartsWith(":30U:")).Split(':')[2].ToString() : "";
                seq15A.SettlementRateSource15A = l.Any(p => p.StartsWith(":14S:")) ? l.First(p => p.StartsWith(":14S:")).Split(':')[2].ToString() : "";
                seq15A.ReferencetoOpeningConfirmation5A = l.Any(p => p.StartsWith(":21A:")) ? l.First(p => p.StartsWith(":21A:")).Split(':')[2].ToString() : "";
                seq15A.ClearingorSettlementSession5A = l.Any(p => p.StartsWith(":14E:")) ? l.First(p => p.StartsWith(":14E:")).Split(':')[2].ToString() : "";

                seq15B.NewSequenceB = "";
                seq15B.TradeDate15B = l.Any(p => p.StartsWith(":30T:")) ? l.First(p => p.StartsWith(":30T:")).Split(':')[2].ToString() : "";
                seq15B.ValueDate15B = l.Any(p => p.StartsWith(":30V:")) ? l.First(p => p.StartsWith(":30V:")).Split(':')[2].ToString() : "";
                seq15B.ExchangeRate15B = l.Any(p => p.StartsWith(":36:")) ? l.First(p => p.StartsWith(":36:")).Split(':')[2].ToString().Replace(',', '.') : "";
                seq15B.CurrencyAmountbought15B = l.Any(p => p.StartsWith(":32B:")) ? l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Trim().Substring(0, 3) : "";
                seq15B.Amountbought15B = l.Any(p => p.StartsWith(":32B:")) ? l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) : l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) + "." + l.First(p => p.StartsWith(":32B:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.IntermediaryAmountbought15B = l.Any(p => p.StartsWith(":56A:")) ? l.First(p => p.StartsWith(":56A:")).Split(':')[2].ToString() : "";

                seq15B.ReceivingAgentAmountbought15B = l.Any(p => p.StartsWith("T:57A:")) ? l.First(p => p.StartsWith("T:57A:")).Split(':')[2].ToString().Split("|")[1] : "";

                seq15B.CurrencyAmountSold15B = l.Any(p => p.StartsWith(":33B:")) ? l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Trim().Substring(0, 3) : "";
                seq15B.AmountSold15B = l.Any(p => p.StartsWith(":33B:")) ? l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") == "" ? l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) : l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Substring(3, l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Split(',')[0].Replace("#", "").Length - 3) + "." + l.First(p => p.StartsWith(":33B:")).Split(':')[2].ToString().Trim().Split(',')[1].Replace("#", "") : "";
                seq15B.DeliveryAgentAmountSold15B = l.Any(p => p.StartsWith(":53A:")) ? l.First(p => p.StartsWith(":53A:")).Split(':')[2].ToString() : "";
                seq15B.IntermediaryAmountSold15B = l.Any(p => p.StartsWith(":56A:")) ? l.First(p => p.StartsWith(":56A:")).Split(':')[2].ToString() : "";
                seq15B.ReceivingAgentAmountSold15B = l.Any(p => p.StartsWith(":57A:")) ? l.First(p => p.StartsWith(":57A:")).Split(':')[2].ToString() : "";
                seq15B.BeneficiaryInstitutionAmountSold15B = l.Any(p => p.StartsWith(":58A:")) ? l.First(p => p.StartsWith(":58A:")).Split(':')[2].ToString() : "";


                seq15C.NewSequenceC = "";
                seq15C.DealingMethod15C = l.Any(p => p.StartsWith(":24D:")) ? l.First(p => p.StartsWith(":24D:")).Split(':')[1].ToString() : "";

                seq15E.NewSequenceE = "";
                seq15E.ExecutionVenue15E = l.Any(p => p.StartsWith("22V:")) ? l.First(p => p.StartsWith("22V:")).Split('|')[1].ToString() : "";
                seq15E.ExecutionTimestamp15E = l.Any(p => p.StartsWith("98D:")) ? l.First(p => p.StartsWith("98D:")).Split('|')[1].ToString() : "";


                scontent = " New Sequence A, Sender's Reference,Type of Operation,Scope of Operation,Common Reference ,  " +
                                "Split Settlement Indicator,Party A - BIC, Party B - BIC, Non - Deliverable Indicator,New Sequence B,   " +
                                " Trade Date,Value Date,Exchange Rate,Currency,Amount,Receiving Agent - FI BIC,Currency,Amount, " +
                               " Delivery Agent - FI BIC ,Receiving Agent - FI BIC," +
                               " New Sequence C,Dealing Method,New Sequence E,Execution Venue,Execution Timestamp" + Environment.NewLine;
                scontent += seq15A.NewSequenceA + "," + seq15A.SenderRef15A + "," + seq15A.TypeofOperation15A + "," + seq15A.ScopeofOperation15A + "," + seq15A.CommonReference15A + "," + seq15A.SplitSettlementIndicator15A + "," + seq15A.PartyA15A + "," + seq15A.PartyB15A + "," + seq15A.NonDeliverableIndicator15A;
                scontent += "," + seq15B.NewSequenceB + "," + seq15B.TradeDate15B + "," + seq15B.ValueDate15B + "," + seq15B.ExchangeRate15B + "," + seq15B.CurrencyAmountbought15B;
                scontent += "," + seq15B.Amountbought15B + "," + seq15B.ReceivingAgentAmountbought15B + "," + seq15B.CurrencyAmountSold15B + "," + seq15B.AmountSold15B + "," + seq15B.DeliveryAgentAmountSold15B + "," + seq15B.ReceivingAgentAmountSold15B;
                scontent += "," + seq15C.NewSequenceC + "," + seq15C.DealingMethod15C + "," + seq15E.NewSequenceE + "," + seq15E.ExecutionVenue15E + "," + seq15E.ExecutionTimestamp15E;
                WriteFile(outputFolder + "\\Converted_MT300_" + Path.GetFileNameWithoutExtension(file) + ".csv", scontent);
            }

        }

        public static void WriteFile(string path, string content)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                    sw.Write(content);
            }
        }
        private List<string> GetRtgsDetails(string[] d)
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
                    if (i < 15)
                    {
                        if (d[i].Trim() != "")
                            l.Add("T" + d[i]);
                    }
                    else
                    {
                        l.Add(d[i]);
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
    }
}
