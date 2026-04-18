namespace HC.LIS.Modules.UserAccess.Application.Users.Login;

public record LoginResultDto(string Token, Guid UserId, string UserEmail, string Role);
