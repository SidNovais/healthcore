namespace HC.LIS.Modules.UserAccess.Application.Users;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string role);
}
