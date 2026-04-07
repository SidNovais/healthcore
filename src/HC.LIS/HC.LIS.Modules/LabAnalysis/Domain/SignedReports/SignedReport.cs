using System;
using System.Collections.Generic;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Rules;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports;

public class SignedReport : AggregateRoot
{
    private Guid _worklistItemId;
    private Guid _orderId;
    private Guid _orderItemId;
    private string _signature = string.Empty;
    private Guid _signedBy;
    private DateTime _createdAt;
    private IReadOnlyCollection<AnalyteResultSnapshot> _analyteSnapshots = [];
    private string? _htmlReportPath;
    private DateTime? _htmlUploadedAt;
    private string? _pdfReportPath;
    private DateTime? _pdfUploadedAt;
    private SignedReportStatus _status = SignedReportStatus.Created;

    public IReadOnlyCollection<AnalyteResultSnapshot> AnalyteSnapshots => _analyteSnapshots;

    private SignedReport() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static SignedReport Create(
        Guid reportId,
        WorklistItemForSigning worklistItem,
        string signature,
        Guid signedBy,
        DateTime createdAt)
    {
        CheckRule(new CannotSignReportWhenNotInReportGeneratedStatusRule(worklistItem.Status));

        SignedReport report = new();
        SignedReportCreatedDomainEvent domainEvent = new(
            reportId, worklistItem.WorklistItemId, worklistItem.OrderId, worklistItem.OrderItemId,
            signature, signedBy, createdAt, worklistItem.AnalyteResults);
        report.Apply(domainEvent);
        report.AddDomainEvent(domainEvent);
        return report;
    }

    public void HtmlUploaded(string htmlReportPath, DateTime uploadedAt)
    {
        CheckRule(new CannotUploadHtmlWhenAlreadyUploadedRule(_status));
        HtmlReportUploadedDomainEvent domainEvent = new(Id, _worklistItemId, htmlReportPath, uploadedAt);
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    public void PdfUploaded(string pdfReportPath, DateTime uploadedAt)
    {
        CheckRule(new CannotUploadPdfWhenAlreadyUploadedRule(_status));
        CheckRule(new CannotUploadPdfWithoutHtmlRule(_status));
        PdfReportUploadedDomainEvent domainEvent = new(Id, _worklistItemId, pdfReportPath, uploadedAt);
        Apply(domainEvent);
        AddDomainEvent(domainEvent);
    }

    private void When(SignedReportCreatedDomainEvent domainEvent)
    {
        Id = domainEvent.ReportId;
        _worklistItemId = domainEvent.WorklistItemId;
        _orderId = domainEvent.OrderId;
        _orderItemId = domainEvent.OrderItemId;
        _signature = domainEvent.Signature;
        _signedBy = domainEvent.SignedBy;
        _createdAt = domainEvent.CreatedAt;
        _analyteSnapshots = domainEvent.AnalyteSnapshots;
        _status = SignedReportStatus.Created;
    }

    private void When(HtmlReportUploadedDomainEvent domainEvent)
    {
        _htmlReportPath = domainEvent.HtmlReportPath;
        _htmlUploadedAt = domainEvent.UploadedAt;
        _status = SignedReportStatus.HtmlUploaded;
    }

    private void When(PdfReportUploadedDomainEvent domainEvent)
    {
        _pdfReportPath = domainEvent.PdfReportPath;
        _pdfUploadedAt = domainEvent.UploadedAt;
        _status = SignedReportStatus.PdfUploaded;
    }
}
