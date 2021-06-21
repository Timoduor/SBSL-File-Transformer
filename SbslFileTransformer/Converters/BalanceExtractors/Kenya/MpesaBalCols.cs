using System;

namespace SbslFileTransformer.Infrastructure.Jobs.Converters
{
    public class MpesaBalCols
    {
        public DateTime BalDate { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
    }
}