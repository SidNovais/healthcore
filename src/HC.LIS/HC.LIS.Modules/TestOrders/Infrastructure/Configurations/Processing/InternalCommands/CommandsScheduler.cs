using Dapper;
using System.Data;
using Newtonsoft.Json;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.InternalCommands;
using HC.Core.Infrastructure.Serialization;
using HC.LIS.Modules.TestOrders.Application.Configuration.Commands;
using HC.LIS.Modules.TestOrders.Application.Contracts;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.InternalCommands;

public class CommandsScheduler(
    ISqlConnectionFactory sqlConnectionFactory,
    IInternalCommandsMapper internalCommandsMapper
) : ICommandsScheduler
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    private readonly IInternalCommandsMapper _internalCommandsMapper = internalCommandsMapper;

    public async Task EnqueueAsync(ICommand command)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to insert internal commands");

        const string sqlInsert = @$"INSERT INTO ""test_orders"".""InternalCommands"" (""Id"", ""EnqueueDate"" , ""Type"", ""Data"") VALUES " +
                                 "(@Id, @EnqueueDate, @Type, @Data)";

        await connection.ExecuteAsync(sqlInsert, new
        {
            command.Id,
            EnqueueDate = SystemClock.Now,
            Type = _internalCommandsMapper.GetNameByType(command.GetType()),
            Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            })
        }).ConfigureAwait(false);
    }

    public async Task EnqueueAsync<T>(ICommand<T> command)
    {
        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to insert internal commands");

        const string sqlInsert = @$"INSERT INTO ""test_orders"".""InternalCommands"" (""Id"", ""EnqueueDate"" , ""Type"", ""Data"") VALUES " +
                                 "(@Id, @EnqueueDate, @Type, @Data)";

        await connection.ExecuteAsync(sqlInsert, new
        {
            command.Id,
            EnqueueDate = SystemClock.Now,
            Type = _internalCommandsMapper.GetNameByType(command.GetType()),
            Data = JsonConvert.SerializeObject(command, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            })
        }).ConfigureAwait(false);
    }
}
