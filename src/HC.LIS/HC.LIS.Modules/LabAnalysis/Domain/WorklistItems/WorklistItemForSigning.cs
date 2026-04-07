using System;
using System.Collections.Generic;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class WorklistItemForSigning
{
    public Guid WorklistItemId { get; }
    public Guid OrderId { get; }
    public Guid OrderItemId { get; }
    public WorklistItemStatus Status { get; }
    public IReadOnlyCollection<AnalyteResultSnapshot> AnalyteResults { get; }

    private WorklistItemForSigning(
        Guid worklistItemId,
        Guid orderId,
        Guid orderItemId,
        WorklistItemStatus status,
        IReadOnlyCollection<AnalyteResultSnapshot> analyteResults)
    {
        WorklistItemId = worklistItemId;
        OrderId = orderId;
        OrderItemId = orderItemId;
        Status = status;
        AnalyteResults = analyteResults;
    }

    public static WorklistItemForSigning From(
        Guid worklistItemId,
        Guid orderId,
        Guid orderItemId,
        WorklistItemStatus status,
        IReadOnlyCollection<AnalyteResultSnapshot> analyteResults)
        => new(worklistItemId, orderId, orderItemId, status, analyteResults);
}
