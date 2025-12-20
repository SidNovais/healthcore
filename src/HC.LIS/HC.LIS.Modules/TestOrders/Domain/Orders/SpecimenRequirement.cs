using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class SpecimenRequirement : ValueObject
{
    public string SpecimenMnemonic { get; }
    public string MaterialType { get; }
    public string ContainerType { get; }
    public string Additive { get; }
    public string ProcessingType { get; }
    public string StorageCondition { get; }
    private SpecimenRequirement(
        string specimenMnemonic,
        string materialType,
        string containerType,
        string additive,
        string processingType,
        string storageCondition
    )
    {
        SpecimenMnemonic = specimenMnemonic;
        MaterialType = materialType;
        ContainerType = containerType;
        Additive = additive;
        ProcessingType = processingType;
        StorageCondition = storageCondition;
    }
    public static SpecimenRequirement Of(
        string specimenMnemonic,
        string materialType,
        string containerType,
        string additive,
        string processingType,
        string storageCondition
    ) => new(
        specimenMnemonic,
        materialType,
        containerType,
        additive,
        processingType,
        storageCondition
    );
}
