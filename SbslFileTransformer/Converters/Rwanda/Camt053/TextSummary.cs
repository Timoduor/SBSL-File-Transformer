using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "TxsSummry")]
    public class TextSummary
    {
        [XmlElement("TtlNtries")] public totalEntries TtlNtries { get; set; }

        [XmlElement("TtlCdtNtries")] public totalCrEntries TtlCdtNtries { get; set; }

        public totalDebitEntries TtlDbtNtries { get; set; }
    }
}