using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "InitgPty")]
    public class initialgParty
    {
        [XmlElement("Agt")] public agt Agt { get; set; }
    }
}