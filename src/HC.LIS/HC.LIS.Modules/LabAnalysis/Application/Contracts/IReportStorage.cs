using System;
using System.Threading;
using System.Threading.Tasks;

namespace HC.LIS.Modules.LabAnalysis.Application.Contracts;

public interface IReportStorage
{
    Task<string> SaveHtmlReportAsync(Guid worklistItemId, string htmlContent, CancellationToken cancellationToken);
    Task<string> SavePdfReportAsync(Guid worklistItemId, byte[] pdfBytes, CancellationToken cancellationToken);
}
