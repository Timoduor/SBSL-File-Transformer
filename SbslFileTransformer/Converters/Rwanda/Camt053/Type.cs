using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Tp")]
    public class Type
    {
        [XmlElement("CdOrPrtry")] public CreditOrderPerEntry CdOrPrtry { get; set; }
    }
}