using HC.LIS.API.Modules.UserAccess.AuditLog.GetAuditLog;
using HC.LIS.Modules.UserAccess.Application.Users.GetAuditLog;

namespace HC.LIS.API.Modules.UserAccess.AuditLog;

internal static class AuditLogEndpoints
{
    internal static RouteGroupBuilder MapAuditLogEndpoints(
        this RouteGroupBuilder group)
    {
        group.WithTags("Audit");

        group.MapGet("", GetAuditLogEndpoint.Handle)
            .RequireAuthorization("ITAdmin")
            .WithName("GetAuditLog")
            .WithSummary("Get audit log entries.")
            .Produces<IReadOnlyCollection<AuditLogEntryDto>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }
}
