using HC.LIS.Modules.UserAccess.Application.Users.GetAuditLog;
using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.API.Modules.UserAccess.AuditLog.GetAuditLog;

internal static class GetAuditLogEndpoint
{
    internal static async Task<IResult> Handle(
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        IUserAccessModule module,
        CancellationToken ct)
    {
        var result = await module.ExecuteQueryAsync(
            new GetAuditLogQuery(userId, fromDate, toDate)).ConfigureAwait(false);

        return TypedResults.Ok(result);
    }
}
