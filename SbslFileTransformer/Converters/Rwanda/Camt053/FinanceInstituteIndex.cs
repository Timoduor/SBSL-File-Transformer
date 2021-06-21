using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "FinInstnId")]
    public class FinanceInstituteIndex
    {
        public string BICFI { get; set; }
    }
}