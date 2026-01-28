using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;

public class RequestExamCommand(
    Guid orderId,
    Guid itemId,
    string specimenMnemonic,
    string materialType,
    string containerType,
    string additive,
    string processingType,
    string storageCondition,
    DateTime requestedAt
) : CommandBase
{
    public Guid OrderId { get; } = orderId;
    public Guid ItemId { get; } = itemId;
    public string SpecimenMnemonic { get; } = specimenMnemonic;
    public string MaterialType { get; } = materialType;
    public string ContainerType { get; } = containerType;
    public string Additive { get; } = additive;
    public string ProcessingType { get; } = processingType;
    public string StorageCondition { get; } = storageCondition;
    public DateTime RequestedAt { get; } = requestedAt;
}
