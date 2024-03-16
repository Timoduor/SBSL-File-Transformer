using System;

namespace SbslFileTransformer.Converters.Kenya
{
    public class FinacleRec
    {
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public string ReferenceNumber { get; set; }
        public string CardNumber { get; set; }
        public string TransDate { get; set; }
        public DateTime ValueDate { get; set; }
        public string TransactionTime { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string DebitCredit { get; set; }
        public double Amount { get; set; }
        public string TransactionParticular { get; set; }
        public string TransactionID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Time { get; set; }
        public string Ref5 { get; set; }
        public string Branch { get; set; }
    }
}
