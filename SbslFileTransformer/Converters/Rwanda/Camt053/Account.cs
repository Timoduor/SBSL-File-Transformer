using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Acct")]
    public class Account
    {
        [XmlElement("Id")] public ID1 Id { get; set; }

        [XmlElement("Ownr")] public Owner Ownr { get; set; }
    }
}