using System;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using HC.Core.Domain;
using Npgsql;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class AnalyzerSampleDetailsProjectorTests : TestBase
{
    public AnalyzerSampleDetailsProjectorTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task ProjectingAnalyzerSampleCreatedTwiceIsIdempotent()
    {
        await AnalyzerSampleFactory.CreateAsync(AnalyzerModule).ConfigureAwait(true);

        await GetEventually(
            new GetAnalyzerSampleDetailsProbe(AnalyzerSampleSampleData.AnalyzerSampleId, AnalyzerModule),
            15000
        ).ConfigureAwait(true);

        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.ExecuteAsync(
                @"UPDATE analyzer.""OutboxMessages"" SET ""ProcessedDate"" = NULL WHERE ""Type"" = 'AnalyzerSampleCreatedNotification'"
            ).ConfigureAwait(true);
        }

        var retryTimeout = SystemClock.Now.AddSeconds(15);
        bool retrySucceeded = false;
        while (!retrySucceeded && SystemClock.Now < retryTimeout)
        {
            await Task.Delay(500).ConfigureAwait(true);
            using var connection = new NpgsqlConnection(ConnectionString);
            DateTime? processedDate = await connection.ExecuteScalarAsync<DateTime?>(
                @"SELECT ""ProcessedDate"" FROM analyzer.""OutboxMessages"" WHERE ""Type"" = 'AnalyzerSampleCreatedNotification'"
            ).ConfigureAwait(true);
            retrySucceeded = processedDate.HasValue;
        }

        retrySucceeded.Should().BeTrue("the retried outbox message should be processed idempotently without a duplicate key exception");

        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            int rowCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM analyzer.analyzer_sample_details WHERE id = @Id",
                new { Id = AnalyzerSampleSampleData.AnalyzerSampleId }
            ).ConfigureAwait(true);
            rowCount.Should().Be(1);
        }
    }
}
