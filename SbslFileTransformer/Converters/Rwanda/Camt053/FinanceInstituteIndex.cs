using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "FinInstnId")]
    public class FinanceInstituteIndex
    {
        public string BICFI { get; set; }
    }
}