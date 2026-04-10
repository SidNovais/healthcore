using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;

namespace HC.LIS.Modules.Analyzer.IntegrationTests.AnalyzerSamples;

public class ReceiveExamResultTests : TestBase
{
    public ReceiveExamResultTests() : base(Guid.CreateVersion7()) { }

    private async Task ArrangeDispatchedSample(IReadOnlyCollection<ExamInfoDto> exams)
    {
        await AnalyzerSampleFactory.CreateAsync(AnalyzerModule, exams).ConfigureAwait(false);

        await GetEventually(
            new GetAnalyzerSampleDetailsProbe(AnalyzerSampleSampleData.AnalyzerSampleId, AnalyzerModule),
            15000
        ).ConfigureAwait(false);

        await AnalyzerModule.ExecuteCommandAsync(new DispatchSampleInfoCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            SystemClock.Now
        )).ConfigureAwait(false);

        await GetEventually(
            new GetAnalyzerSampleDetailsProbe(
                AnalyzerSampleSampleData.AnalyzerSampleId,
                AnalyzerModule,
                dto => dto?.Status == "InfoDispatched"),
            15000
        ).ConfigureAwait(false);
    }

    [Fact]
    public async Task ReceiveExamResultIsSuccessful()
    {
        await ArrangeDispatchedSample(
            new List<ExamInfoDto>
            {
                new(AnalyzerSampleSampleData.ExamId1, AnalyzerSampleSampleData.ExamMnemonic1)
            }.AsReadOnly()
        ).ConfigureAwait(true);

        await AnalyzerModule.ExecuteCommandAsync(new ReceiveExamResultCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            AnalyzerSampleSampleData.ExamMnemonic1,
            AnalyzerSampleSampleData.ResultValue,
            AnalyzerSampleSampleData.ResultUnit,
            AnalyzerSampleSampleData.ReferenceRange,
            AnalyzerSampleSampleData.InstrumentId,
            SystemClock.Now
        )).ConfigureAwait(true);

        IReadOnlyCollection<AnalyzerSampleExamDetailsDto>? exams = await GetEventually(
            new GetAnalyzerSampleExamDetailsProbe(
                AnalyzerSampleSampleData.AnalyzerSampleId,
                AnalyzerModule,
                dtos => dtos?.Any(e => e.ResultValue == AnalyzerSampleSampleData.ResultValue) == true),
            15000
        ).ConfigureAwait(true);

        exams.Should().NotBeNull();
        AnalyzerSampleExamDetailsDto exam = exams!.Should().ContainSingle().Subject;
        exam.ExamMnemonic.Should().Be(AnalyzerSampleSampleData.ExamMnemonic1);
        exam.ResultValue.Should().Be(AnalyzerSampleSampleData.ResultValue);
        exam.ResultUnit.Should().Be(AnalyzerSampleSampleData.ResultUnit);
        exam.ReferenceRange.Should().Be(AnalyzerSampleSampleData.ReferenceRange);
        exam.InstrumentId.Should().Be(AnalyzerSampleSampleData.InstrumentId);
        exam.RecordedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ReceiveAllExamResultsCompletesAnalyzerSample()
    {
        await ArrangeDispatchedSample(
            new List<ExamInfoDto>
            {
                new(AnalyzerSampleSampleData.ExamId1, AnalyzerSampleSampleData.ExamMnemonic1),
                new(AnalyzerSampleSampleData.ExamId2, AnalyzerSampleSampleData.ExamMnemonic2)
            }.AsReadOnly()
        ).ConfigureAwait(true);

        await AnalyzerModule.ExecuteCommandAsync(new ReceiveExamResultCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            AnalyzerSampleSampleData.ExamMnemonic1,
            AnalyzerSampleSampleData.ResultValue,
            AnalyzerSampleSampleData.ResultUnit,
            AnalyzerSampleSampleData.ReferenceRange,
            AnalyzerSampleSampleData.InstrumentId,
            SystemClock.Now
        )).ConfigureAwait(true);

        await AnalyzerModule.ExecuteCommandAsync(new ReceiveExamResultCommand(
            AnalyzerSampleSampleData.AnalyzerSampleId,
            AnalyzerSampleSampleData.ExamMnemonic2,
            "4.2",
            "mmol/L",
            "3.5-5.5",
            AnalyzerSampleSampleData.InstrumentId,
            SystemClock.Now
        )).ConfigureAwait(true);

        AnalyzerSampleDetailsDto? details = await GetEventually(
            new GetAnalyzerSampleDetailsProbe(
                AnalyzerSampleSampleData.AnalyzerSampleId,
                AnalyzerModule,
                dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("ResultReceived");
    }
}
