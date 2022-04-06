using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    public class BookToCredit
    {
        [XmlElement("Prtry")] public PerEntry Prtry { get; set; }
    }
}