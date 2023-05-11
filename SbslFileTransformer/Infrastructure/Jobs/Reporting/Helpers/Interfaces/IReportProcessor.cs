using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers.Interfaces
{
    public interface IReportProcessor
    {
        Task ProcessFetchedReportsAsync(Dictionary<string, IEnumerable<ReportModel>> unprocessedReports, IProgress<int> processReportProgress);
    }
}
