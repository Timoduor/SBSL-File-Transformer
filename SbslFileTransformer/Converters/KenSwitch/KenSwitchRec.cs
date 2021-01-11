using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SbslFileTransformer.Converters.KenSwitch
{
    public class KenSwitchRec
    {
        public string TerminalId { get; set; }
        public string NameLocation { get; set; }
        public string AcquirerIssuer { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string CardNo { get; set; }
        public string FromAcc { get; set; }
        public string ToAcc { get; set; }
        public string RRN1 { get; set; }
        public string RRN2 { get; set; }
        public string Stip { get; set; }
        public string PartRev { get; set; }
        public string Amount { get; set; }
    }

    public enum KenSwitchFileType
    {
        ATMActivity,
        ClientDebitActivity,
    }
}
