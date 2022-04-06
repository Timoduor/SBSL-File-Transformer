using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "InitgPty")]
    public class initialgParty
    {
        [XmlElement("Agt")] public agt Agt { get; set; }
    }
}