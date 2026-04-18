namespace HC.LIS.Modules.UserAccess.Application.Users.GetUserDetails;

public record UserDetailsDto(
    Guid Id,
    string Email,
    string FullName,
    DateOnly Birthdate,
    string Gender,
    string Role,
    string Status,
    DateTime CreatedAt,
    Guid? CreatedById,
    DateTime? ActivatedAt);
