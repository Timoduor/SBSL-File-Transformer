using System.Xml.Serialization;
using SbslFileTransformer.Converters.Camt053;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "TxDtls")]
    public class TextDetails
    {
        [XmlElement("Refs")] public Reference Refs { get; set; }

        public string Amt { get; set; }

        public string CdtDbtInd { get; set; }

        [XmlElement("RltdPties")] public Retailedparties RltdPties { get; set; }
    }
}