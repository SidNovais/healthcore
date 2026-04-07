using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Rules;

public class PdfUploadWithoutHtmlException : BaseBusinessRuleException
{
    public PdfUploadWithoutHtmlException(string message) : base(message) { }
    public PdfUploadWithoutHtmlException(string message, System.Exception innerException) : base(message, innerException) { }
    public PdfUploadWithoutHtmlException(IBusinessRule rule) : base(rule) { }
    public PdfUploadWithoutHtmlException() { }
}

public class CannotUploadPdfWithoutHtmlRule(
    SignedReportStatus actualStatus
) : IBusinessRule
{
    private readonly SignedReportStatus _actualStatus = actualStatus;
    public bool IsBroken() => !_actualStatus.IsHtmlUploaded;
    public void ThrowException() => throw new PdfUploadWithoutHtmlException(this);
    public string Message => "Cannot upload PDF report: HTML must be uploaded before PDF";
}
