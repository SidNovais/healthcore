using System.Data;
using System.Reflection;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Inbox;

internal class ProcessInboxCommandHandler(
  IMediator mediator,
  ISqlConnectionFactory sqlConnectionFactory
) : ICommandHandler<ProcessInboxCommand>
{
    private readonly IMediator _mediator = mediator;
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Handle(ProcessInboxCommand command, CancellationToken cancellationToken)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to update internal commands");
        string sql = @$"SELECT
                   ""InboxMessage"".""Id"" AS ""{nameof(InboxMessageDto.Id)}"",
                   ""InboxMessage"".""Type"" AS ""{nameof(InboxMessageDto.Type)}"",
                   ""InboxMessage"".""Data"" AS ""{nameof(InboxMessageDto.Data)}""
                   FROM ""test_orders"".""InboxMessages"" AS ""InboxMessage""
                   WHERE ""InboxMessage"".""ProcessedDate"" IS NULL
                   ORDER BY ""InboxMessage"".""OccurredAt""";

        IEnumerable<InboxMessageDto> messages =
            await connection.QueryAsync<InboxMessageDto>(sql).ConfigureAwait(false);

        const string sqlUpdateProcessedDate = @$"UPDATE ""test_orders"".""InboxMessages""
                                            SET ""ProcessedDate"" = @Date
                                            WHERE ""Id"" = @Id";

        foreach (InboxMessageDto? message in messages)
        {
            Assembly? messageAssembly;
            if (message is not null && message.Type is not null)
            {
                messageAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(
                        assembly => message.Type.Contains(assembly.GetName().Name ?? string.Empty, StringComparison.Ordinal)
                    );
                Type? type = null;
                if (messageAssembly is not null && message.Type is not null)
                    type = messageAssembly.GetType(message.Type);
                if (type is not null)
                {
                    object? request = JsonConvert.DeserializeObject(message.Data, type);

                    if (request is not null)
                        try
                        {
                            await _mediator.Publish((INotification)request, cancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    await connection.ExecuteScalarAsync(sqlUpdateProcessedDate, new
                    {
                        Date = SystemClock.Now,
                        message.Id
                    })
                    .ConfigureAwait(false);
                }
            }
        }
    }
}
