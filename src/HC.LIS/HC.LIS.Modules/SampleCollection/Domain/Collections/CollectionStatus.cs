using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class CollectionStatus : ValueObject
{
    public string Value { get; }
    public static CollectionStatus Arrived => new("Arrived");
    public static CollectionStatus Waiting => new("Waiting");
    public static CollectionStatus Called => new("Called");
    private CollectionStatus(string value)
    {
        Value = value;
    }
    public static CollectionStatus Of(string value) => new(value);
    internal bool IsArrived => Value == "Arrived";
    internal bool IsWaiting => Value == "Waiting";
    internal bool IsCalled => Value == "Called";
}
