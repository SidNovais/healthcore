using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HC.Core.Infrastructure.DomainEventsDispatching;

namespace HC.Core.Infrastructure;

public class UnitOfWork(
    DbContext context,
    IDomainEventsDispatcher domainEventsDispatcher
) : IUnitOfWork
{
    private readonly DbContext _dbContext = context;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher = domainEventsDispatcher;
    public async Task<int> CommitAsync(
        Guid? internalCommandId,
        CancellationToken cancellationToken = default
    )
    {
        await _domainEventsDispatcher.DispatchEventsAsync().ConfigureAwait(false);
        Task<int> task = _dbContext.SaveChangesAsync(cancellationToken);
        return await task.ConfigureAwait(false);
    }
}
