using System;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Models
{
    public class OpenItem
    {
        //public string Account { get; set; }
        public string Entity { get; set; }
        public string AccName { get; set; }
        public DateTime PostedDate { get; set; }
        public int DaysOverdue { get; set; }
        public string Amount { get; set; }
        public string ItemSubType { get; set; }
        public string WeBalance { get; set; }
        public string TheyBalance { get; set; }
        public string ItemSide { get; set; }
        public string TransNarrative { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Reference3 { get; set; }
        public string FunctionalArea { get; set; }
        public string ActiveCertStatus { get; set; }
        public string ItemId { get; set; }
        public string Column16 { get; set; }
        public string Column17 { get; set; }
        public string Column18 { get; set; }
        public string Column19 { get; set; }
        public string Column20 { get; set; }
    }
}
