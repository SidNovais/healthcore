namespace HC.LIS.Modules.UserAccess.Application.Users.GetAuditLog;

public record AuditLogEntryDto(
    Guid Id,
    DateTime OccurredAt,
    Guid? UserId,
    Guid? ActorId,
    string EventType,
    string? Details);
