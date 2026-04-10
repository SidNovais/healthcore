using System;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class DispatchSampleInfoTests : TestBase
{
    public DispatchSampleInfoTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task DispatchSampleInfoIsSuccessful()
    {
        await AnalyzerSampleFactory.CreateAsync(AnalyzerModule).ConfigureAwait(true);

        await GetEventually(
            new GetAnalyzerSampleDetailsProbe(AnalyzerSampleSampleData.AnalyzerSampleId, AnalyzerModule),
            15000
        ).ConfigureAwait(true);

        await AnalyzerModule.ExecuteCommandAsync(new DispatchSampleInfoCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            SystemClock.Now
        )).ConfigureAwait(true);

        AnalyzerSampleDetailsDto? details = await GetEventually(
            new GetAnalyzerSampleDetailsProbe(
                AnalyzerSampleSampleData.AnalyzerSampleId,
                AnalyzerModule,
                dto => dto?.Status == "InfoDispatched"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("InfoDispatched");
        details.DispatchedAt.Should().NotBeNull();
    }
}
