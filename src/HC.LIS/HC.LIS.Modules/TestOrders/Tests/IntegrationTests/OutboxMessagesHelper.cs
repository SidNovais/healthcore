using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using HC.LIS.Modules.TestOrders.Application;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;
using MediatR;
using Newtonsoft.Json;

namespace HC.LIS.Modules.TestOrders.IntegrationTests;

public class OutboxMessagesHelper
{
    public static async Task<List<OutboxMessageDto>> GetOutboxMessages(IDbConnection connection)
    {
        const string sql = @"SELECT
                         ""OutboxMessage"".""Id"",
                         ""OutboxMessage"".""Type"",
                         ""OutboxMessage"".""Data""
                         FROM ""test_orders"".""OutboxMessages"" AS ""OutboxMessage""
                         ORDER BY ""OutboxMessage"".""OccurredAt""";

        IEnumerable<OutboxMessageDto> messages = await connection.QueryAsync<OutboxMessageDto>(sql).ConfigureAwait(false);
        return messages.AsList();
    }

    public static T? Deserialize<T>(OutboxMessageDto message)
        where T : class, INotification
    {
        Type? notificationType = typeof(ApplicationAssemblyInfo);
        string? genericType = typeof(T).FullName;
        if (genericType is not null)
        {
            Type? desserializeType = Assembly.GetAssembly(notificationType)?.GetType(genericType);
            if (desserializeType is not null) return JsonConvert.DeserializeObject(message.Data, desserializeType) as T;
        }
        return null;
    }
}
