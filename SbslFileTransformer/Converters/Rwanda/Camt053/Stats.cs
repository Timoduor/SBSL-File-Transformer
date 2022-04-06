using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Sts")]
    public class Stats
    {
        public string Cd { get; set; }
    }
}