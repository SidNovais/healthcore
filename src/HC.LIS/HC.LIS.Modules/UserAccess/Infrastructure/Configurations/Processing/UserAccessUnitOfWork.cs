using HC.Core.Infrastructure;
using HC.Core.Infrastructure.DomainEventsDispatching;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing;

public class UserAccessUnitOfWork(
    IDomainEventsDispatcher domainEventsDispatcher,
    UserAccessContext context
) : IUnitOfWork
{
    private readonly IDomainEventsDispatcher _domainEventsDispatcher = domainEventsDispatcher;
    private readonly UserAccessContext _context = context;

    public async Task<int> CommitAsync(
        Guid? internalCommandId = null,
        CancellationToken cancellationToken = default)
    {
        await _domainEventsDispatcher.DispatchEventsAsync().ConfigureAwait(false);
        return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
