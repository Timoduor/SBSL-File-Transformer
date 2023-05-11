using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.HelpersV2.Interfaces
{
    public interface IReportProcessor
    {
        Task ProcessFetchedReportsAsync(Dictionary<string, IEnumerable<ReportModel>> unprocessedReports, IProgress<int> processReportProgress);
    }
}
