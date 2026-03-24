using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.SampleCollection.Domain.Collections;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;

internal class CollectionRequestDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(PatientArrivedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO sample_collection.""CollectionRequestDetails""
              (""Id"", ""PatientId"", ""Status"", ""ArrivedAt"")
              VALUES (@CollectionRequestId, @PatientId, @Status, @ArrivedAt)",
            new
            {
                e.CollectionRequestId,
                e.PatientId,
                Status = CollectionStatus.Arrived.Value,
                e.ArrivedAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PatientWaitingDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE sample_collection.""CollectionRequestDetails""
              SET ""Status"" = @Status, ""WaitingAt"" = @WaitingAt
              WHERE ""Id"" = @CollectionRequestId",
            new
            {
                e.CollectionRequestId,
                Status = CollectionStatus.Waiting.Value,
                e.WaitingAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PatientCalledDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE sample_collection.""CollectionRequestDetails""
              SET ""Status"" = @Status, ""CalledAt"" = @CalledAt
              WHERE ""Id"" = @CollectionRequestId",
            new
            {
                e.CollectionRequestId,
                Status = CollectionStatus.Called.Value,
                e.CalledAt
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
