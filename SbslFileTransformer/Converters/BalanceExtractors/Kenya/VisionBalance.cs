using System;

namespace SbslFileTransformer.Converters.BalanceExtractors.Kenya
{
    public class VisionBalance
    {
        public DateTime BankingDate { get; set; }
        public string ContractNumber { get; set; }
        public string CardNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ClientShortName { get; set; }
        public int Currency { get; set; }
        public double Balance { get; set; }
        public double TotalBalance { get; set; }
        public double AvailableBalance { get; set; }
        public double CardLimit { get; set; }
        public string Product { get; set; }
        public int DelCount { get; set; }
        public double FxRate { get; set; }
        public string Account { get; set; }
    }
}
