using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Refs")]
    public class Reference
    {
        public string MsgId { get; set; }
        public string AcctSvcrRef { get; set; }
        public string InstrId { get; set; }
        public string EndToEndId { get; set; }
        public string TxId { get; set; }
    }
}