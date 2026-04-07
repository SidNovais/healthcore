using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Reports;

internal class S3ReportStorage(
    IAmazonS3 s3Client,
    S3ReportStorageOptions options
) : IReportStorage
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly S3ReportStorageOptions _options = options;

    public async Task<string> SaveHtmlReportAsync(Guid worklistItemId, string htmlContent, CancellationToken cancellationToken)
    {
        string key = $"reports/html/{worklistItemId:N}.html";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(htmlContent));
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = "text/html; charset=utf-8",
            CannedACL = S3CannedACL.PublicRead
        }, cancellationToken).ConfigureAwait(false);

        return BuildObjectUrl(key);
    }

    public async Task<string> SavePdfReportAsync(Guid worklistItemId, byte[] pdfBytes, CancellationToken cancellationToken)
    {
        string key = $"reports/pdf/{worklistItemId:N}.pdf";
        using var stream = new MemoryStream(pdfBytes);
        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = "application/pdf",
            CannedACL = S3CannedACL.PublicRead
        }, cancellationToken).ConfigureAwait(false);

        return BuildObjectUrl(key);
    }

    private string BuildObjectUrl(string key) =>
        $"https://{_options.BucketName}.s3.{_options.Region}.amazonaws.com/{key}";
}
