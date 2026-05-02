namespace HC.LIS.API.Modules.UserAccess.Auth.Login;

internal sealed record LoginRequest(
    string Email,
    string Password);
