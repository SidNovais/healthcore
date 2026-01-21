using MediatR;
using Newtonsoft.Json;
using System.Data;
using Dapper;
using Serilog.Context;
using Serilog.Events;
using Serilog.Core;
using HC.Core.Domain;
using HC.Core.Application;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.DomainEventsDispatching;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.Core.Application.Events;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;

internal class ProcessOutboxCommandHandler(
    IMediator mediator,
    ISqlConnectionFactory sqlConnectionFactory,
    IDomainNotificationsMapper domainNotificationsMapper
) : ICommandHandler<ProcessOutboxCommand>
{
    private readonly IMediator _mediator = mediator;

    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    private readonly IDomainNotificationsMapper _domainNotificationsMapper = domainNotificationsMapper;

    public async Task Handle(
        ProcessOutboxCommand command,
        CancellationToken cancellationToken
    )
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to update outbox messages");
        string sql = @$"SELECT
                    ""OutboxMessage"".""Id"" AS ""{nameof(OutboxMessageDto.Id)}"",
                    ""OutboxMessage"".""Type"" AS ""{nameof(OutboxMessageDto.Type)}"",
                    ""OutboxMessage"".""Data"" AS ""{nameof(OutboxMessageDto.Data)}""
                    FROM ""test_orders"".""OutboxMessages"" AS ""OutboxMessage""
                    WHERE ""OutboxMessage"".""ProcessedDate"" IS NULL
                    ORDER BY ""OutboxMessage"".""OccurredAt""";

        var messages = await connection.QueryAsync<OutboxMessageDto>(sql).ConfigureAwait(false);
        var messagesList = messages.AsList();

        const string sqlUpdateProcessedDate = @$"UPDATE ""test_orders"".""OutboxMessages""
                                            SET ""ProcessedDate"" = @Date
                                            WHERE ""Id"" = @Id";
        if (messagesList.Count > 0)
        {
            foreach (OutboxMessageDto? message in messagesList)
            {
                Type? type = _domainNotificationsMapper.GetTypeByName(message.Type);
                if (type is not null)
                {
                    var @event = JsonConvert.DeserializeObject(message.Data, type) as IDomainEventNotification;

                    if (@event is not null)
                        using (LogContext.Push(new OutboxMessageContextEnricher(@event)))
                        {
                            await _mediator.Publish(@event, cancellationToken).ConfigureAwait(false);

                            await connection.ExecuteAsync(sqlUpdateProcessedDate, new
                            {
                                Date = SystemClock.Now,
                                message.Id
                            }).ConfigureAwait(false);
                        }
                }
            }
        }
    }

    private class OutboxMessageContextEnricher(IDomainEventNotification notification) : ILogEventEnricher
    {
        private readonly IDomainEventNotification _notification = notification;

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty("Context", new ScalarValue($"OutboxMessage:{_notification.Id}")));
        }
    }
}
