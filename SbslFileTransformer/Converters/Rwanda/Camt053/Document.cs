using System;
using System.Xml;
using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Document")]
    public class Document
    {
        [XmlElement("BkToCstmrStmt")] public BankToCustomer BkStmt { get; set; }

        internal XmlNodeList GetElementsByTagName(string v)
        {
            throw new NotImplementedException();
        }
    }
}