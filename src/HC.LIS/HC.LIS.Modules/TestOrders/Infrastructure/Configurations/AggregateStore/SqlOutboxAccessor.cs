using Marten;
using HC.Core.Infrastructure.Outbox;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.AggregateStore;

public class SqlOutboxAccessor(
  IDocumentSession documentSession
) : IOutbox
{
    private readonly IDocumentSession _documentSession = documentSession;
    private readonly List<OutboxMessage> _messages = [];
    public void Add(OutboxMessage message)
    {
        _messages.Add(message);
    }

    public Task Save()
    {
        if (_messages.Count != 0)
        {
            const string sql = @"INSERT INTO ""test_orders"".""OutboxMessages""
                      (""Id"", ""OccurredAt"", ""Type"", ""Data"")
                      VALUES (?::uuid, ?::timestamptz, ?, ?::jsonb)";
            foreach (OutboxMessage message in _messages)
                _documentSession.QueueSqlCommand(sql,
                  message.Id,
                  message.OccurredAt,
                  message.Type!,
                  message.Data!
                );
            _messages.Clear();
        }
        return Task.CompletedTask;
    }
}
