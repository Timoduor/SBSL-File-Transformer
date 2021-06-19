using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Sts")]
    public class Stats
    {
        public string Cd { get; set; }
    }
}