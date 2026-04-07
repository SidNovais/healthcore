using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Rules;

public class PdfAlreadyUploadedException : BaseBusinessRuleException
{
    public PdfAlreadyUploadedException(string message) : base(message) { }
    public PdfAlreadyUploadedException(string message, System.Exception innerException) : base(message, innerException) { }
    public PdfAlreadyUploadedException(IBusinessRule rule) : base(rule) { }
    public PdfAlreadyUploadedException() { }
}

public class CannotUploadPdfWhenAlreadyUploadedRule(
    SignedReportStatus actualStatus
) : IBusinessRule
{
    private readonly SignedReportStatus _actualStatus = actualStatus;
    public bool IsBroken() => _actualStatus.IsPdfUploaded;
    public void ThrowException() => throw new PdfAlreadyUploadedException(this);
    public string Message => "Cannot upload PDF report: PDF has already been uploaded for this signed report";
}
