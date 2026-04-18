namespace HC.LIS.Modules.UserAccess.Application.Users;

public interface IAuditLogWriter
{
    Task WriteAsync(Guid? userId, Guid? actorId, string eventType, string? details);
}
