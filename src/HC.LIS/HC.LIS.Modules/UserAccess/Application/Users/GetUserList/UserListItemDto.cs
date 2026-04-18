namespace HC.LIS.Modules.UserAccess.Application.Users.GetUserList;

public record UserListItemDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string Status,
    DateTime CreatedAt);
