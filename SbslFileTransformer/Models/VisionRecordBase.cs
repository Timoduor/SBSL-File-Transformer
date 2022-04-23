using System;

namespace SbslFileTransformer.Models
{
    public abstract class VisionRecordBase
    {
        public Guid Id { get; set; }
        public DateTime BankingDate { get; set; }
        public string TransDetails { get; set; }
        public string TransID { get; set; }
        public string ReferenceNumber { get; set; }
        public string GLTransCode { get; set; }
        public string CardNumber { get; set; }
        public double CreditAmount { get; set; }
        public double DebitAmount { get; set; }
        public string CustomerName { get; set; }
        public string ContractNumber { get; set; }
        public string AccountNumber { get; set; }
        public bool Matched { get; set; }
        public string FileName { get; set; }
        public DateTime DateExtracted { get; set; }
        public DateTime? DateMatched { get; set; }
        public string MatchingFile { get; set; }
    }
}
