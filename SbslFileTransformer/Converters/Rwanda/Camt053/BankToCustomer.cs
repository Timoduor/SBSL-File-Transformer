using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "BkToCstmrStmt")]
    public class BankToCustomer
    {
        [XmlElement("Stmt")] public Statement Stmt { get; set; }
    }
}