using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.SampleCollection.Domain.Collections;
using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;

internal class SampleDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(SampleCreatedForExamDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO sample_collection.""SampleDetails""
              (""Id"", ""CollectionRequestId"", ""TubeType"", ""Status"")
              VALUES (@SampleId, @CollectionRequestId, @TubeType, @Status)",
            new
            {
                e.SampleId,
                e.CollectionRequestId,
                e.TubeType,
                Status = SampleStatus.Pending.Value
            }
        ).ConfigureAwait(false);
    }

    private async Task When(BarcodeCreatedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE sample_collection.""SampleDetails""
              SET ""Barcode"" = @BarcodeValue, ""Status"" = @Status
              WHERE ""Id"" = @SampleId",
            new
            {
                e.SampleId,
                e.BarcodeValue,
                Status = SampleStatus.BarcodeCreated.Value
            }
        ).ConfigureAwait(false);
    }

    private async Task When(SampleCollectedDomainEvent e)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE sample_collection.""SampleDetails""
              SET ""Status"" = @Status, ""CollectedAt"" = @CollectedAt
              WHERE ""Id"" = @SampleId",
            new
            {
                e.SampleId,
                Status = SampleStatus.Collected.Value,
                e.CollectedAt
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
