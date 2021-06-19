using System;

namespace SbslFileTransformer.Converters
{
    public class W2BCols
    {
        public DateTime Date { get; set; }
        public string Processed { get; set; }
        public string TransID { get; set; }
        public string Reference { get; set; }
        public string Terminal { get; set; }
        public string Account2 { get; set; }
        public string Result { get; set; }
        public double Amount { get; set; }
        public string Channel { get; set; }
        public string Account { get; set; }
    }
}