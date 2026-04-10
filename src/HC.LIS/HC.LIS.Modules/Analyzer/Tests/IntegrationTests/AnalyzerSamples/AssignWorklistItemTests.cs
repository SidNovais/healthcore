using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class AssignWorklistItemTests : TestBase
{
    public AssignWorklistItemTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task AssignWorklistItemIsSuccessful()
    {
        await AnalyzerSampleFactory.CreateAsync(AnalyzerModule).ConfigureAwait(true);

        await GetEventually(
            new GetAnalyzerSampleExamDetailsProbe(AnalyzerSampleSampleData.AnalyzerSampleId, AnalyzerModule),
            15000
        ).ConfigureAwait(true);

        await AnalyzerModule.ExecuteCommandAsync(new AssignWorklistItemCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            AnalyzerSampleSampleData.ExamMnemonic1,
            AnalyzerSampleSampleData.WorklistItemId,
            HC.Core.Domain.SystemClock.Now
        )).ConfigureAwait(true);

        IReadOnlyCollection<AnalyzerSampleExamDetailsDto>? exams = await GetEventually(
            new GetAnalyzerSampleExamDetailsProbe(
                AnalyzerSampleSampleData.AnalyzerSampleId,
                AnalyzerModule,
                dtos => dtos?.Any(e => e.WorklistItemId == AnalyzerSampleSampleData.WorklistItemId) == true),
            15000
        ).ConfigureAwait(true);

        exams.Should().NotBeNull();
        AnalyzerSampleExamDetailsDto exam = exams!.Should().ContainSingle().Subject;
        exam.ExamMnemonic.Should().Be(AnalyzerSampleSampleData.ExamMnemonic1);
        exam.WorklistItemId.Should().Be(AnalyzerSampleSampleData.WorklistItemId);
    }
}
