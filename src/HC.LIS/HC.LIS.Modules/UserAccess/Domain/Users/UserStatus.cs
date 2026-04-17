using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users;

public class UserStatus : ValueObject
{
    public string Value { get; }

    public static UserStatus PendingActivation => new("PendingActivation");
    public static UserStatus Active => new("Active");

    private UserStatus(string value)
    {
        Value = value;
    }

    public static UserStatus Of(string value) => new(value);

    internal bool IsPendingActivation => Value == "PendingActivation";
    internal bool IsActive => Value == "Active";
}
