namespace SbslFileTransformer.Converters.Kenya.Models
{
    public class EP75Item
    {
        public string BatchNo { get; set; }
        public string TranDate { get; set; }
        public string TranTime { get; set; }
        public string CardNo { get; set; }
        public string ReferenceNo { get; set; }
        public string TraceNo { get; set; }
        public string IssuerDetails { get; set; }
        public string TranType { get; set; }
        public string ProcessCode { get; set; }
        public string EntryMode { get; set; }
        public string ReasonCode { get; set; }
        public string RspCode { get; set; }
        public decimal TranAmount { get; set; }
        public string Currency { get; set; }
        public string SettledAmount { get; set; }

        public string DrCr { get; set; }
        public string Terminal { get; set; }
    }
}
