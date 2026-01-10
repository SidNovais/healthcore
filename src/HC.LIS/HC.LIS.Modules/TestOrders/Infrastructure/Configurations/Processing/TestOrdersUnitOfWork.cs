using HC.Core.Infrastructure;
using HC.Core.Infrastructure.DomainEventsDispatching;
using HC.Core.Infrastructure.Outbox;
using Marten;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

public class TestOrdersUnitOfWork(
  IDomainEventsDispatcher domainEventsDispatcher,
  IDocumentSession documentSession,
  IOutbox outbox
) : IUnitOfWork
{
    private readonly IDomainEventsDispatcher _domainEventsDispatcher = domainEventsDispatcher;
    private readonly IDocumentSession _documentSession = documentSession;
    private readonly IOutbox _outbox = outbox;
    public async Task<int> CommitAsync(
    Guid? internalCommandId = null,
    CancellationToken cancellationToken = default
  )
    {
        await _domainEventsDispatcher.DispatchEventsAsync().ConfigureAwait(false);
        await _outbox.Save().ConfigureAwait(false);
        if (internalCommandId.HasValue)
        {
            _documentSession.QueueSqlCommand(
              @$"UPDATE ""test_orders"".""InternalCommands""
                SET ""ProcessedDate"" = ?
                WHERE ""Id"" = ?",
              DateTimeOffset.UtcNow,
              internalCommandId.Value
            );
        }
        await _documentSession.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return 0;
    }
}
