using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports;

public class SignedReportStatus : ValueObject
{
    public string Value { get; }

    public static SignedReportStatus Created     => new("Created");
    public static SignedReportStatus HtmlUploaded => new("HtmlUploaded");
    public static SignedReportStatus PdfUploaded  => new("PdfUploaded");

    private SignedReportStatus(string value) => Value = value;

    internal bool IsCreated      => Value == "Created";
    internal bool IsHtmlUploaded => Value == "HtmlUploaded";
    internal bool IsPdfUploaded  => Value == "PdfUploaded";
}
