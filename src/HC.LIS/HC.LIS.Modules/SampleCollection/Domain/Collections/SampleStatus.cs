using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class SampleStatus : ValueObject
{
    public string Value { get; }
    public static SampleStatus Pending => new("Pending");
    public static SampleStatus BarcodeCreated => new("BarcodeCreated");
    public static SampleStatus Collected => new("Collected");
    private SampleStatus(string value)
    {
        Value = value;
    }
    public static SampleStatus Of(string value) => new(value);
    internal bool IsPending => Value == "Pending";
    internal bool IsBarcodeCreated => Value == "BarcodeCreated";
    internal bool IsCollected => Value == "Collected";
}
