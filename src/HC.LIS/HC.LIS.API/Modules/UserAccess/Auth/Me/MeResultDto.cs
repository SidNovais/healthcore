namespace HC.LIS.API.Modules.UserAccess.Auth.CurrentUser;

internal record MeResultDto(Guid UserId, string UserName, string Role);
