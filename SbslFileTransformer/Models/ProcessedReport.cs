using System;

namespace SbslFileTransformer.Models
{
    public class ProcessedReport
    {
        public long Id { get; set; }
        public long ReportId { get; set; }
        public DateTime ProcessedDate { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public string Creator { get; set; }
        public string EndTime { get; set; }
        public string Message { get; set; }
        public string Notes { get; set; }
        public string StartTime { get; set; }
        public string Status { get; set; }
        public string UserToken { get; set; }
    }
}
