using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;

public class GetUserDetailsQuery(Guid? userId = null, string? email = null) : QueryBase<UserDetailsDto?>
{
    public Guid? UserId { get; } = userId;
    public string? Email { get; } = email;
}
