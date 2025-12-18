using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class SpecimenType : ValueObject
{
    public string Value { get; }
    public static SpecimenType Blood => new("Blood");
    public static SpecimenType Serum => new("Serum");
    public static SpecimenType Urine => new("Urine");
    public static SpecimenType Saliva => new("Saliva");
    public static SpecimenType Swab => new("Swab");
    private SpecimenType(string value)
    {
        Value = value;
    }
    public static SpecimenType Of(string value) => new(value);
    internal bool IsBlood => Value == "Blood";
    internal bool IsSerum => Value == "Serum";
    internal bool IsUrine => Value == "Urine";
    internal bool IsSaliva => Value == "Saliva";
    internal bool IsSwab => Value == "Swab";
}
