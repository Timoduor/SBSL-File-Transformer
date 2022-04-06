using System;

namespace SbslFileTransformer.Converters.BalanceExtractors
{
    public class CdmCols
    {
        public DateTime ReconDate { get; set; }
        public string Account { get; set; }
        public double AmountMC { get; set; }
        public double AmountGL { get; set; }
    }
}