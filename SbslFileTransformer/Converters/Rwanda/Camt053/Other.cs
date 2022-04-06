using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Othr")]
    public class Other
    {
        [XmlElement("Id")] public string Id { get; set; }
    }
}