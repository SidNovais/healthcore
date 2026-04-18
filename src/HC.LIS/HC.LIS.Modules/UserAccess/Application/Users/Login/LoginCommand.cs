using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

public class LoginCommand(string email, string password) : CommandBase<LoginResultDto>
{
    public string Email { get; } = email;
    public string Password { get; } = password;
}
