namespace HC.LIS.API.Modules.UserAccess.Users.CreateUser;

internal sealed record CreateUserRequest(
    string Email,
    string FullName,
    DateTime Birthdate,
    string Gender,
    string Role);
