using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Ntry")]
    public class NumberEntry
    {
        public string NtryRef { get; set; }
        public string Amt { get; set; }
        public string CdtDbtInd { get; set; }

        [XmlElement("Sts")] public Stats Sts { get; set; }

        [XmlElement("ValDt")] public Valdation ValDt { get; set; }

        [XmlElement("BkTxCd")] public BookToCredit BkTxCd { get; set; }

        [XmlElement("NtryDtls")] public EntryDetails NtryDtls { get; set; }
    }
}