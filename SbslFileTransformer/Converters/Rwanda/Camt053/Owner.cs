using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Ownr")]
    public class Owner
    {
        [XmlElement("Id")] public ID2 Id { get; set; }
    }
}