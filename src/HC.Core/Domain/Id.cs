using System;

namespace HC.Core.Domain;

public abstract class Id : IEquatable<Id>
{
    public Guid Value { get; }

    protected Id(Guid value)
    {
        if (value == Guid.Empty)
            throw new InvalidOperationException("Id.Value cannot be empty");
        Value = value;
    }

    public override int GetHashCode() => Value.GetHashCode();

    public bool Equals(Id? otherId) => Value == otherId?.Value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Id other && Equals(other);
    }

    public static bool operator ==(Id firstId, Id secondId)
    {
        if (object.Equals(firstId, null)) return object.Equals(secondId, null);
        return firstId.Equals(secondId);
    }

    public static bool operator !=(Id firstId, Id secondId) => !(firstId == secondId);
}
