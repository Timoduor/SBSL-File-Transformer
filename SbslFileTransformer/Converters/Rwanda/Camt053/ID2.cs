using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "Id")]
    public class ID2
    {
        [XmlElement("OrgId")] public OrganisationId OrgId { get; set; }
    }
}