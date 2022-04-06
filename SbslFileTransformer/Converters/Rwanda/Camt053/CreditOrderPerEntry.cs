using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Cd")]
    public class CreditOrderPerEntry
    {
        [XmlElement("Cd")] public string Cd { get; set; }
    }
}