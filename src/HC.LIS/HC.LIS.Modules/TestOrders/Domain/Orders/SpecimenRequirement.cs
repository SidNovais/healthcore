using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class SpecimenRequirement : ValueObject
{
    public string SpecimenMnemonic { get; }
    public string MaterialType { get; }
    public string ContainerType { get; }
    private SpecimenRequirement(
        string specimenMnemonic,
        string materialType,
        string containerType
    )
    {
        SpecimenMnemonic = specimenMnemonic;
        MaterialType = materialType;
        ContainerType = containerType;
    }
    public static SpecimenRequirement Of(
        string specimenMnemonic,
        string materialType,
        string containerType
    ) => new(
        specimenMnemonic,
        materialType,
        containerType
    );
}
