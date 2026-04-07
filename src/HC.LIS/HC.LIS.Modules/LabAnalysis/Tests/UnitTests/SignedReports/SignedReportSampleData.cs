using System;
using System.Collections.Generic;
using HC.Core.Domain;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.SignedReports;

internal readonly struct SignedReportSampleData
{
    public static readonly Guid ReportId       = Guid.Parse("019b6642-6c05-7678-abcd-2bd510a95e50");
    public static readonly Guid WorklistItemId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e50");
    public static readonly Guid OrderId        = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550e2");
    public static readonly Guid OrderItemId    = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550f3");
    public const string Signature              = "Dr. John Doe";
    public static readonly Guid SignedBy       = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5031");
    public static readonly DateTime CreatedAt  = new(2026, 4, 6, 12, 0, 0, DateTimeKind.Utc);
    public const string HtmlReportPath        = "https://bucket.s3.us-east-1.amazonaws.com/reports/html/019b6642.html";
    public const string PdfReportPath         = "https://bucket.s3.us-east-1.amazonaws.com/reports/pdf/019b6642.pdf";
    public static readonly DateTime HtmlUploadedAt = new(2026, 4, 6, 12, 10, 0, DateTimeKind.Utc);
    public static readonly DateTime PdfUploadedAt  = new(2026, 4, 6, 12, 11, 0, DateTimeKind.Utc);

    public static readonly IReadOnlyCollection<AnalyteResultSnapshot> AnalyteSnapshots =
    [
        new AnalyteResultSnapshot("GLUCOSE", "5.5", "mmol/L", "3.9-6.1", false),
        new AnalyteResultSnapshot("CREATININE", "110", "µmol/L", "62-115", false),
    ];
}
