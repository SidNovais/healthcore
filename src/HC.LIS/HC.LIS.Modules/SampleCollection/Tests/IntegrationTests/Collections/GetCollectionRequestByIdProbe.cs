using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

public class GetCollectionRequestByIdProbe(
    Guid expectedId,
    string? connectionString
) : IProbe<CollectionRequestDetailsDto>
{
    private readonly Guid _expectedId = expectedId;
    private readonly string? _connectionString = connectionString;

    public string DescribeFailureTo() =>
        $"CollectionRequestDetails not found for Id {_expectedId}";

    public async Task<CollectionRequestDetailsDto?> GetSampleAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<CollectionRequestDetailsDto>(
            @"SELECT ""Id"" AS ""CollectionRequestId"", ""PatientId"", ""Status"", ""ArrivedAt""
              FROM sample_collection.""CollectionRequestDetails""
              WHERE ""Id"" = @Id",
            new { Id = _expectedId }
        ).ConfigureAwait(false);
    }

    public bool IsSatisfied(CollectionRequestDetailsDto? sample) => sample is not null;
}
