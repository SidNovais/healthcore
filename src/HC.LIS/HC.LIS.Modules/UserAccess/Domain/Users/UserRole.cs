using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.Domain.Users;

public class UserRole : ValueObject
{
    public string Value { get; }

    public static UserRole LabTechnician => new("LabTechnician");
    public static UserRole ITAdmin => new("ITAdmin");
    public static UserRole Physician => new("Physician");
    public static UserRole Receptionist => new("Receptionist");

    private UserRole(string value)
    {
        Value = value;
    }

    public static UserRole Of(string value) => new(value);
}
