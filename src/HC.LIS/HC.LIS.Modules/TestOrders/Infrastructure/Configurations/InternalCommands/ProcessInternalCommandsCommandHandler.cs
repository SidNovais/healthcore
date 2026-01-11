using Dapper;
using Polly;
using System.Data;
using Newtonsoft.Json;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.InternalCommands;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.InternalCommands;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

internal class ProcessInternalCommandsCommandHandler(
    ISqlConnectionFactory sqlConnectionFactory,
    IInternalCommandsMapper internalCommandsMapper
) : ICommandHandler<ProcessInternalCommandsCommand>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    private readonly IInternalCommandsMapper _internalCommandsMapper = internalCommandsMapper;

    public async Task Handle(ProcessInternalCommandsCommand command, CancellationToken cancellationToken)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to update internal commands");
        string sql = @$"SELECT
                    ""Command"".""Id"" AS ""{nameof(InternalCommandDto.Id)}"",
                    ""Command"".""Type"" AS ""{nameof(InternalCommandDto.Type)}"",
                    ""Command"".""Data"" AS ""{nameof(InternalCommandDto.Data)}""
                    FROM ""test_orders"".""InternalCommands"" AS ""Command""
                    WHERE ""Command"".""ProcessedDate"" IS NULL
                    ORDER BY ""Command"".""EnqueueDate""";

        IEnumerable<InternalCommandDto> commands =
            await connection.QueryAsync<InternalCommandDto>(sql).ConfigureAwait(false);

        var internalCommandsList = commands.AsList();
        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
            [
              TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(3)
            ]);
        foreach (InternalCommandDto? internalCommand in internalCommandsList)
        {
            PolicyResult result = await policy.ExecuteAndCaptureAsync(() => ProcessCommand(
                internalCommand)).ConfigureAwait(false);

            if (result.Outcome == OutcomeType.Failure)
            {
                const string updateOnErrorSql = @$"UPDATE ""test_orders"".""InternalCommands""
                                          SET ""ProcessedDate"" = @NowDate,
                                          ""Error"" = @Error
                                          WHERE ""Id"" = @Id";

                await connection.ExecuteScalarAsync(
                    updateOnErrorSql,
                    new
                    {
                        NowDate = SystemClock.Now,
                        Error = result.FinalException.ToString(),
                        internalCommand.Id
                    }
                ).ConfigureAwait(false);
            }
        }
    }

    private async Task ProcessCommand(
        InternalCommandDto internalCommand)
    {
        Type? type = _internalCommandsMapper.GetTypeByName(internalCommand.Type);
        if (type is not null)
        {
            dynamic? commandToProcess = JsonConvert.DeserializeObject(internalCommand.Data, type);
            await CommandsExecutor.Execute(commandToProcess);
        }
    }

    private class InternalCommandDto
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Data { get; set; }
    }
}
