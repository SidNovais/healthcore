using System;
using System.Data;
using FluentAssertions;
using HC.Core.Infastructure;

namespace HC.Core.IntegrationTests;

[Collection("IntegrationTests")]
public class SqlConnectionFactoryTests
{
    private static string ConnectionString =>
        EnvironmentVariablesProvider.GetVariable("ASPNETCORE_HCLIS_IntegrationTests_ConnectionString")
        ?? throw new InvalidOperationException(
            "Missing ASPNETCORE_HCLIS_IntegrationTests_ConnectionString environment variable.");

    // Reproduces the outbox/inbox connection leak: a processing handler holds a
    // scope-managed connection via GetConnection(), then a nested projector borrows a
    // transient connection via CreateConnection() and disposes it. CreateConnection()
    // must NOT touch the cached connection — otherwise Dispose() closes the wrong one
    // and the scope-managed connection is never returned to the pool.
    [Fact]
    public void CreateConnectionDoesNotLeakTheScopeManagedConnection()
    {
        var factory = new SqlConnectionFactory(ConnectionString);

        IDbConnection scoped = factory.GetConnection()!;

        using (IDbConnection transient = factory.CreateConnection())
        {
            transient.State.Should().Be(ConnectionState.Open);
            transient.Should().NotBeSameAs(scoped);
        }

        // Disposing the transient must leave the scope-managed connection intact.
        factory.GetConnection().Should().BeSameAs(scoped);
        scoped.State.Should().Be(ConnectionState.Open);

        // End of DI scope: the factory is disposed and MUST close the scope-managed
        // connection (returning it to the pool) rather than a stale transient slot.
        factory.Dispose();
        scoped.State.Should().Be(ConnectionState.Closed);
    }
}
