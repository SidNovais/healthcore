using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

namespace HC.LIS.Modules.TestOrders.Application.Physicians.GetPhysicianDetails;

internal class PhysicianDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(PhysicianRegisteredDomainEvent physicianRegistered)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO test_orders.""PhysicianDetails""
              (""Id"", ""FullName"", ""LicenceNumber"", ""Status"", ""RegisteredAt"")
              VALUES (@PhysicianId, @FullName, @LicenceNumber, 'Active', @RegisteredAt)
              ON CONFLICT (""Id"") DO NOTHING",
            new
            {
                physicianRegistered.PhysicianId,
                physicianRegistered.FullName,
                physicianRegistered.LicenceNumber,
                physicianRegistered.RegisteredAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PhysicianUpdatedDomainEvent physicianUpdated)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE test_orders.""PhysicianDetails""
              SET ""FullName"" = @FullName,
                  ""LicenceNumber"" = @LicenceNumber,
                  ""UpdatedAt"" = @UpdatedAt
              WHERE ""Id"" = @PhysicianId",
            new
            {
                physicianUpdated.PhysicianId,
                physicianUpdated.FullName,
                physicianUpdated.LicenceNumber,
                physicianUpdated.UpdatedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PhysicianDeactivatedDomainEvent physicianDeactivated)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE test_orders.""PhysicianDetails""
              SET ""Status"" = 'Inactive',
                  ""DeactivatedAt"" = @DeactivatedAt
              WHERE ""Id"" = @PhysicianId",
            new
            {
                physicianDeactivated.PhysicianId,
                physicianDeactivated.DeactivatedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PhysicianReactivatedDomainEvent physicianReactivated)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE test_orders.""PhysicianDetails""
              SET ""Status"" = 'Active',
                  ""DeactivatedAt"" = NULL,
                  ""UpdatedAt"" = @ReactivatedAt
              WHERE ""Id"" = @PhysicianId",
            new
            {
                physicianReactivated.PhysicianId,
                physicianReactivated.ReactivatedAt
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
