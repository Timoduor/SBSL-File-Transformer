using SbslFileTransformer.Models;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Models
{
    public class EscalationReport
    {
        public string OverdueReportPath { get; set; }
        public ReportModel OriginalReport { get; set; }
        public ReportConfiguration Escalation { get; set; }
    }
}
