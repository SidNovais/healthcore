namespace HC.LIS.API.Modules.UserAccess.Users.ActivateUser;

internal sealed record ActivateUserRequest(
    string InvitationToken,
    string Password);
