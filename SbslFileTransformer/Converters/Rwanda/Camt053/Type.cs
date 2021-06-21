using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Tp")]
    public class Type
    {
        [XmlElement("CdOrPrtry")] public CreditOrderPerEntry CdOrPrtry { get; set; }
    }
}