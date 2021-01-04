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
    }
}
