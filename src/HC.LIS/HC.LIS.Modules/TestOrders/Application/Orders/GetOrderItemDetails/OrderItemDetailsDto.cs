namespace HC.LIS.Modules.TestOrders.Application.Orders.GetOrderItemDetails;

public class OrderItemDetailsDto(
    Guid orderItemId,
    Guid orderId,
    string specimenMnemonic,
    string materialType,
    string containerType,
    string additive,
    string processingType,
    string storageCondition,
    string status,
    string reasonForRejection,
    DateTime requestedAt,
    DateTime? canceledAt = null,
    DateTime? onHoldAt = null,
    DateTime? acceptedAt = null,
    DateTime? rejectedAt = null,
    DateTime? inProgressAt = null,
    DateTime? partiallyCompletedAt = null,
    DateTime? completedAt = null
)
{
    public Guid OrderId { get; } = orderId;
    public Guid OrderItemId { get; } = orderItemId;
    public string SpecimenMnemonic { get; } = specimenMnemonic;
    public string MaterialType { get; } = materialType;
    public string ContainerType { get; } = containerType;
    public string Additive { get; } = additive;
    public string ProcessingType { get; } = processingType;
    public string StorageCondition { get; } = storageCondition;
    public string ReasonForRejection { get; } = reasonForRejection;
    public string Status { get; } = status;
    public DateTime RequestedAt { get; } = requestedAt;
    public DateTime? CanceledAt { get; } = canceledAt;
    public DateTime? OnHoldAt { get; } = onHoldAt;
    public DateTime? AcceptedAt { get; } = acceptedAt;
    public DateTime? RejectedAt { get; } = rejectedAt;
    public DateTime? InProgressAt { get; } = inProgressAt;
    public DateTime? PartiallyCompletedAt { get; } = partiallyCompletedAt;
    public DateTime? CompletedAt { get; } = completedAt;
}
