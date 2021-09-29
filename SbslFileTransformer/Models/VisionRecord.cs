using System;

namespace SbslFileTransformer.Models
{
    public class VisionRecord
    {
        public DateTime BankingDate { get; set; }
        public string TransDetails { get; set; }
        public string TransID { get; set; }
        public string ReferenceNo { get; set; }
        public string GLTransCode { get; set; }
        public string CardNo { get; set; }
        public double CreditAmount { get; set; }
        public double DebitAmount { get; set; }
        public string CustomerName { get; set; }
        public string ContractNumber { get; set; }
        public string AccountNumber { get; set; }
        public string FileName { get; set; }
        public DateTime DateProcessed { get; set; }
    }
}
