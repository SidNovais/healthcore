using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class CreateAnalyzerSampleTests : TestBase
{
    public CreateAnalyzerSampleTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task CreateAnalyzerSampleIsSuccessful()
    {
        await AnalyzerSampleFactory.CreateAsync(
            AnalyzerModule,
            new List<ExamInfoDto>
            {
                new(AnalyzerSampleSampleData.ExamId1, AnalyzerSampleSampleData.ExamMnemonic1),
                new(AnalyzerSampleSampleData.ExamId2, AnalyzerSampleSampleData.ExamMnemonic2)
            }.AsReadOnly()
        ).ConfigureAwait(true);

        AnalyzerSampleDetailsDto? details = await GetEventually(
            new GetAnalyzerSampleDetailsProbe(AnalyzerSampleSampleData.AnalyzerSampleId, AnalyzerModule),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Id.Should().Be(AnalyzerSampleSampleData.AnalyzerSampleId);
        details.SampleId.Should().Be(AnalyzerSampleSampleData.SampleId);
        details.PatientId.Should().Be(AnalyzerSampleSampleData.PatientId);
        details.SampleBarcode.Should().Be(AnalyzerSampleSampleData.SampleBarcode);
        details.PatientName.Should().Be(AnalyzerSampleSampleData.PatientName);
        details.PatientGender.Should().Be(AnalyzerSampleSampleData.PatientGender);
        details.Status.Should().Be("AwaitingQuery");
        details.DispatchedAt.Should().BeNull();

        IReadOnlyCollection<AnalyzerSampleExamDetailsDto>? exams = await GetEventually(
            new GetAnalyzerSampleExamDetailsProbe(
                AnalyzerSampleSampleData.AnalyzerSampleId,
                AnalyzerModule,
                dtos => dtos?.Count == 2),
            15000
        ).ConfigureAwait(true);

        exams.Should().NotBeNull();
        exams!.Should().HaveCount(2);
        exams!.Select(e => e.ExamMnemonic).Should().Contain(
            [AnalyzerSampleSampleData.ExamMnemonic1, AnalyzerSampleSampleData.ExamMnemonic2]);
        exams.Should().AllSatisfy(e =>
        {
            e.WorklistItemId.Should().BeNull();
            e.ResultValue.Should().BeNull();
        });
    }
}
