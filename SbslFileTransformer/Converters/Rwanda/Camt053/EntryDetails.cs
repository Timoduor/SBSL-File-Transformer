using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "NtryDtls")]
    public class EntryDetails
    {
        [XmlElement("TxDtls")] public TextDetails TxDtls { get; set; }
    }
}