using System.Data;
using Autofac;
using Dapper;
using Newtonsoft.Json;
using HC.Core.Infrastructure.Data;
using HC.Core.Infrastructure.EventBus;
using HC.Core.Infrastructure.Serialization;
using HC.COre.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.EventBus;

internal class IntegrationEventGenericHandler<T> : IIntegrationEventListener<T>
    where T : IntegrationEvent
{
    public async Task Handle(T integrationEvent)
    {
        using ILifetimeScope scope = TestOrdersCompositionRoot.BeginLifetimeScope();
        using IDbConnection? connection = scope.Resolve<ISqlConnectionFactory>().GetConnection()
        ?? throw new InvalidOperationException("Must exist connection to insert inbox messages");
        string? type = integrationEvent.GetType().FullName;
        var data = JsonConvert.SerializeObject(integrationEvent, new JsonSerializerSettings
        {
            ContractResolver = new AllPropertiesContractResolver()
        });

        string sql = @$"INSERT INTO ""test_orders"".""InboxMessages"" (""Id"", ""OccurredAt"", ""Type"", ""Data"") " +
                  "VALUES (@Id, @OccurredAt, @Type, @Data)";

        await connection.ExecuteScalarAsync(sql, new
        {
            integrationEvent.Id,
            integrationEvent.OccurredAt,
            type,
            data
        }).ConfigureAwait(false);
    }
}
