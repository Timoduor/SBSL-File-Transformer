using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Camt053
{
    [XmlRoot(ElementName = "Id")]
    public class ID2
    {
        [XmlElement("OrgId")] public OrganisationId OrgId { get; set; }
    }
}