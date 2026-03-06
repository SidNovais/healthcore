#!/bin/bash
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Use: ./create-integration-test-layer.sh [ModuleName] [RootNamespace] [BaseModulesDir]"
  exit 1
fi

camel_to_snake() {
  local input="$1"
  echo "$input" | sed -E 's/([A-Z])/_\L\1/g' | sed 's/^_//'
}

MODULE_NAME=$1
ROOT_NS=$2
BASE_MODULES_DIR=$3
MODULE_NAME_SNAKE_FORMAT=$(camel_to_snake "$MODULE_NAME")
BASE_MODULE_DIR=${BASE_MODULES_DIR}/${MODULE_NAME}/Tests

mkdir -p ${BASE_MODULE_DIR}/IntegrationTests

cat > "${BASE_MODULE_DIR}/IntegrationTests/${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationTests.csproj" << 'CSPROJEOF'
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="NSubstitute" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
</Project>
CSPROJEOF

cat > "${BASE_MODULE_DIR}/IntegrationTests/TestBase.cs" << EOF
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using MediatR;
using Npgsql;
using Serilog;
using HC.Core.Application;
using HC.Core.Domain;
using HC.Core.IntegrationTests;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application.Contracts;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationTests;

[Collection("IntegrationTests")]
public class TestBase
{
    protected string? ConnectionString { get; private set; }
    protected ILogger Logger { get; private set; }
    protected I${MODULE_NAME}Module ${MODULE_NAME}Module { get; private set; }
    protected IExecutionContextAccessor ExecutionContextAccessor { get; private set; }

    public TestBase(Guid UserId, string RoleScopeType = "Customer")
    {
        const string connectionStringEnvironmentVariable = "ASPNETCORE_HCLIS_IntegrationTests_ConnectionString";
        ConnectionString = EnvironmentVariablesProvider.GetVariable(connectionStringEnvironmentVariable);
        if (ConnectionString == null)
        {
            throw new InvalidOperationException(
                \$"Define connection string to integration tests database using environment variable: {connectionStringEnvironmentVariable}");
        }

        using (var sqlConnection = new NpgsqlConnection(ConnectionString))
        {
            ClearDatabase(sqlConnection).GetAwaiter().GetResult();
        }

        Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .CreateLogger();

        ExecutionContextAccessor = new ExecutionContextMock(UserId, "user");
        ${MODULE_NAME}Startup.Initialize(
            ConnectionString,
            ExecutionContextAccessor,
            Logger,
            null
        );
        ${MODULE_NAME}Module = new ${MODULE_NAME}Module();
    }

    protected static void AssertBrokenRule<TRule>(Action testDelegate)
        where TRule : class, IBusinessRule
    {
        testDelegate.Should().Throw<BaseBusinessRuleException>().Which
            .Rule.Should().BeOfType<TRule>();
    }

    protected async Task<T?> GetLastOutboxMessage<T>()
        where T : class, INotification
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            var messages = await OutboxMessagesHelper.GetOutboxMessages(connection).ConfigureAwait(false);
            return OutboxMessagesHelper.Deserialize<T>(messages.Last(x => x.Type == typeof(T).Name));
        }
    }

    private static async Task ClearDatabase(IDbConnection connection)
    {
        const string sql = @"DELETE FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""InboxMessages"";
                             DELETE FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""InternalCommands"";
                             DELETE FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""OutboxMessages"";";

        await connection.ExecuteScalarAsync(sql).ConfigureAwait(false);
    }
}
EOF

cat > "${BASE_MODULE_DIR}/IntegrationTests/ExecutionContextMock.cs" << EOF
using System;
using HC.Core.Application;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationTests;

public class ExecutionContextMock(Guid userId, string userName) : IExecutionContextAccessor
{
    public Guid UserId { get; } = userId;
    public string UserName { get; } = userName;
    public string CorrelationId { get; } = string.Empty;
    public bool IsAvailable { get; } = true;
}
EOF

cat > "${BASE_MODULE_DIR}/IntegrationTests/OutboxMessagesHelper.cs" << EOF
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Newtonsoft.Json;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Application;
using ${ROOT_NS}.Modules.${MODULE_NAME}.Infrastructure.Configurations.Processing.Outbox;

namespace ${ROOT_NS}.Modules.${MODULE_NAME}.IntegrationTests;

public class OutboxMessagesHelper
{
    public static async Task<IReadOnlyCollection<OutboxMessageDto>> GetOutboxMessages(IDbConnection connection)
    {
        const string sql = @"SELECT
                             ""OutboxMessage"".""Id"",
                             ""OutboxMessage"".""Type"",
                             ""OutboxMessage"".""Data""
                             FROM ""${MODULE_NAME_SNAKE_FORMAT}"".""OutboxMessages"" AS ""OutboxMessage""
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
EOF
