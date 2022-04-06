using System;

namespace SbslFileTransformer.Converters.BalanceExtractors.Kenya
{
    public class MpesaBalCols
    {
        public DateTime BalDate { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
    }
}