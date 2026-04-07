using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Rules;

public class HtmlAlreadyUploadedException : BaseBusinessRuleException
{
    public HtmlAlreadyUploadedException(string message) : base(message) { }
    public HtmlAlreadyUploadedException(string message, System.Exception innerException) : base(message, innerException) { }
    public HtmlAlreadyUploadedException(IBusinessRule rule) : base(rule) { }
    public HtmlAlreadyUploadedException() { }
}

public class CannotUploadHtmlWhenAlreadyUploadedRule(
    SignedReportStatus actualStatus
) : IBusinessRule
{
    private readonly SignedReportStatus _actualStatus = actualStatus;
    public bool IsBroken() => !_actualStatus.IsCreated;
    public void ThrowException() => throw new HtmlAlreadyUploadedException(this);
    public string Message => "Cannot upload HTML report: HTML has already been uploaded for this signed report";
}
