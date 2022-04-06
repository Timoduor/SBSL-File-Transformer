using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class totalEntries
    {
        public string NbOfNtries { get; set; }

        public string Sum { get; set; }

        [XmlElement("TtlNetNtry")] public ToatalNetEntry TtlNetNtry { get; set; }
    }
}