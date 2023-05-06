using System;
using System.Collections.Generic;
using SbslFileTransformer.Models.Enums;

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
        public DateTime ReportDate { get; set; }
        public string UserToken { get; set; }
        public Country Country { get; set; } = Country.Kenya;
        public Sprint Sprint { get; set; } = Sprint.Nostro;
        public ReportCategory Category { get; set; } = ReportCategory.Default;
        public string TempReportPath { get; set; }
        public int[] DaysRange { get; set; }

        public string ModifiedReportPath { get; set; } 

        public List<OpenItem> ReportContent { get; set; }
    }
}
