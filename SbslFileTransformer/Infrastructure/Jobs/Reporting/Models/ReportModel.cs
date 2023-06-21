using System;
using System.Collections.Generic;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Models
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

        public long Size { get; set; }

        public DateTime ReportDate { get; set; }

        public string UserToken { get; set; }

        public string TempReportPath { get; set; }

        public string ModifiedReportPath { get; set; }

        public List<OpenItem> ReportContent { get; set; }
    }
}
