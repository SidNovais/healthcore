using System;
using System.Collections.Generic;
using HC.Core.Domain;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;

public class SignedReportCreatedDomainEvent(
    Guid reportId,
    Guid worklistItemId,
    Guid orderId,
    Guid orderItemId,
    string signature,
    Guid signedBy,
    DateTime createdAt,
    IReadOnlyCollection<AnalyteResultSnapshot> analyteSnapshots
) : DomainEvent
{
    public Guid ReportId { get; } = reportId;
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid OrderId { get; } = orderId;
    public Guid OrderItemId { get; } = orderItemId;
    public string Signature { get; } = signature;
    public Guid SignedBy { get; } = signedBy;
    public DateTime CreatedAt { get; } = createdAt;
    public IReadOnlyCollection<AnalyteResultSnapshot> AnalyteSnapshots { get; } = analyteSnapshots;
}
