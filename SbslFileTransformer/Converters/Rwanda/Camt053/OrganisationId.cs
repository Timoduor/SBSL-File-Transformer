using System.Xml.Serialization;

namespace SbslFileTransformer.Converters.Rwanda.Camt053
{
    [XmlRoot(ElementName = "OrgId")]
    public class OrganisationId
    {
        public string AnyBIC { get; set; }
    }
}