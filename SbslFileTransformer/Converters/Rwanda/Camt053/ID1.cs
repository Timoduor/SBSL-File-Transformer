using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Id")]
    public class ID1
    {
        [XmlElement("Othr")] public Other Othr { get; set; }
    }
}