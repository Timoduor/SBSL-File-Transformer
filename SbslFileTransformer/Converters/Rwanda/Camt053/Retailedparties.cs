using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "RltdPties")]
    public class Retailedparties
    {
        [XmlElement("InitgPty")] public initialgParty InitgPty { get; set; }
    }
}