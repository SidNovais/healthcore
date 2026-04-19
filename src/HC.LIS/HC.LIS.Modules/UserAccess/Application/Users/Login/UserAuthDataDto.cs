namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

internal record UserAuthDataDto(Guid Id, string Email, string Role, string? PasswordHash);
