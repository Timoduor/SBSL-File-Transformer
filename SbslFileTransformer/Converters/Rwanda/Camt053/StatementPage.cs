using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "StmtPgntn")]
    public class StatementPage
    {
        [XmlElement("PgNb")] public string PgNb { get; set; }

        [XmlElement("LastPgInd")] public string LastPgInd { get; set; }
    }
}