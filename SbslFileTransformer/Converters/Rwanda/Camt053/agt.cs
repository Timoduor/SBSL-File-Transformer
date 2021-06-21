using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "InitgPty")]
    public class agt
    {
        [XmlElement("FinInstnId")] public FinanceInstituteIndex FinInstnId { get; set; }
    }
}