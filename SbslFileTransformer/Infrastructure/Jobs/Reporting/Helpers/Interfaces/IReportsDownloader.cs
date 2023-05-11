using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SbslFileTransformer.Infrastructure.Jobs.Reporting.Models;
using SbslFileTransformer.Models.ViewModels;

namespace SbslFileTransformer.Infrastructure.Jobs.Reporting.Helpers.Interfaces
{
    public interface IReportsDownloader
    {
        Task<Dictionary<string, IEnumerable<ReportModel>>> GetUnprocessedReportListAsync(List<long> processedReportsIds, IProgress<int> progressReporter);

        Task<bool> DownloadReportAndUpdateLocalPath(ReportModel report);

        ReportConfigModel LoadReportConnectionConfig();
    }
}
