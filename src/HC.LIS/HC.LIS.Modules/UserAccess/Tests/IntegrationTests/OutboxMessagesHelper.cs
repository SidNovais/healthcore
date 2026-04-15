using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using HC.LIS.Modules.UserAccess.Application;
using HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing.Outbox;

namespace HC.LIS.Modules.UserAccess.IntegrationTests;

public class OutboxMessagesHelper
{
    public static async Task<IReadOnlyCollection<OutboxMessageDto>> GetOutboxMessages(IDbConnection connection)
    {
        const string sql = @"SELECT
                             ""OutboxMessage"".""Id"",
                             ""OutboxMessage"".""Type"",
                             ""OutboxMessage"".""Data""
                             FROM ""user_access"".""OutboxMessages"" AS ""OutboxMessage""
                             ORDER BY ""OutboxMessage"".""OccurredAt""";

        var messages = await connection.QueryAsync<OutboxMessageDto>(sql).ConfigureAwait(false);
        return messages.AsList().AsReadOnly();
    }

    public static T? Deserialize<T>(OutboxMessageDto message)
        where T : class, INotification
    {
        string? genericType = typeof(T).FullName;
        if (genericType is not null)
        {
            Type? desserializeType = Assembly.GetAssembly(typeof(ApplicationAssemblyInfo))?.GetType(genericType);
            if (desserializeType is not null) return JsonConvert.DeserializeObject(message.Data, desserializeType) as T;
        }
        return null;
    }
}
