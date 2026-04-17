using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users;

public class UserEmail : ValueObject
{
    public string Value { get; }

    private UserEmail(string value)
    {
        Value = value;
    }

    public static UserEmail Of(string value) => new(value);
}
