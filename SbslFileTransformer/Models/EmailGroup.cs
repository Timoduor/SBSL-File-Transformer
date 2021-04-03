using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Models
{
    public class EmailGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public string Emails { get; set; }
        public int AgeAlertDuration { get; set; }
        public string Account { get; set; }
        public Country Country { get; set; }
        public Sprint Sprint { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
