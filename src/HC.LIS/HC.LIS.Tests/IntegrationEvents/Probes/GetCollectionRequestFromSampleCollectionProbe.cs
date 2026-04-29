using System;
using System.Threading.Tasks;
using Dapper;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.SampleCollection.Application.Collections.GetCollectionRequestDetails;
using Npgsql;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetCollectionRequestFromSampleCollectionProbe(
    Guid patientId,
    string connectionString
) : IProbe<CollectionRequestDetailsDto>
{
    public string DescribeFailureTo() =>
        $"CollectionRequest for PatientId {patientId} not found in SampleCollection";

    public async Task<CollectionRequestDetailsDto?> GetSampleAsync()
    {
        using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<CollectionRequestDetailsDto>(
            @"SELECT ""Id"" AS ""CollectionRequestId"", ""PatientId"", ""Status"", ""ArrivedAt""
              FROM sample_collection.""CollectionRequestDetails""
              WHERE ""PatientId"" = @PatientId",
            new { PatientId = patientId }
        ).ConfigureAwait(false);
    }

    public bool IsSatisfied(CollectionRequestDetailsDto? sample) => sample is not null;
}
