using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Bal")]
    public class Balance
    {
        [XmlElement("Tp")] public Type Tp { get; set; }

        public string Amt { get; set; }
        public string CdtDbtInd { get; set; }

        [XmlElement("Dt")] public Date Dt { get; set; }
    }
}