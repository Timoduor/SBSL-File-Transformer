using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "InitgPty")]
    public class agt
    {
        [XmlElement("FinInstnId")] public FinanceInstituteIndex FinInstnId { get; set; }
    }
}