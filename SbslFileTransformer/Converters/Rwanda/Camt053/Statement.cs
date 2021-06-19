using System.Collections.Generic;
using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Stmt")]
    public class Statement
    {
        [XmlElement("Id")] public string Id { get; set; }

        [XmlElement("StmtPgntn")] public StatementPage StmtPgntn { get; set; }

        [XmlElement("Acct")] public Account Account { get; set; }

        public string ElctrncSeqNb { get; set; }

        public string CreDtTm { get; set; }

        [XmlElement("Bal")] public List<Balance> Bal { get; set; }

        [XmlElement("TxsSummry")] public TextSummary TxsSummry { get; set; }

        [XmlElement("Ntry")] public List<NumberEntry> Ntry { get; set; }
    }
}