using System.Collections.Generic;
using HC.LIS.Modules.UserAccess.Application.Contracts;

namespace HC.LIS.Modules.UserAccess.Application.Users.GetAuditLog;

public class GetAuditLogQuery(
    Guid? userId = null,
    DateTime? fromDate = null,
    DateTime? toDate = null) : QueryBase<IReadOnlyCollection<AuditLogEntryDto>>
{
    public Guid? UserId { get; } = userId;
    public DateTime? FromDate { get; } = fromDate;
    public DateTime? ToDate { get; } = toDate;
}
