using SbslFileTransformer.Models.Enums;

namespace SbslFileTransformer.Infrastructure.Jobs
{
    public class ReportModel
    {
        public string Creator { get; set; }
        public string EndTime { get; set; }
        public long ReportId { get; set; }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public string UserToken { get; set; }
        public Country Country { get; set; }
        public Sprint Sprint { get; set; }
        public ReportCategory Category { get; set; }
    }
}