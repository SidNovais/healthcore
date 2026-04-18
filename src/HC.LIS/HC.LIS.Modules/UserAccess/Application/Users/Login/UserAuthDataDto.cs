namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

public record UserAuthDataDto(Guid Id, string Email, string Role, string? PasswordHash);
