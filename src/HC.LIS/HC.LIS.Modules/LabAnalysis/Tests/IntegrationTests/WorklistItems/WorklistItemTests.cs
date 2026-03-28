using System;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GenerateReport;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests.WorklistItems;

public class WorklistItemTests : TestBase
{
    private static readonly Guid WorklistItemId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95f01");
    private static readonly Guid SampleId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550e0");
    private const string SampleBarcode = "SC-INT-001";
    private const string ExamCode = "019b6c5d-fbf9-7e35-aa12-c38922ec5040";
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550f0");
    private static readonly Guid AnalystId = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5041");

    public WorklistItemTests() : base(Guid.CreateVersion7()) { }

    [Fact]
    public async Task CreateWorklistItemIsSuccessful()
    {
        await LabAnalysisModule.ExecuteCommandAsync(new CreateWorklistItemCommand(
            WorklistItemId,
            SampleId,
            SampleBarcode,
            ExamCode,
            PatientId,
            SystemClock.Now
        )).ConfigureAwait(true);

        WorklistItemDetailsDto? details = await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(WorklistItemId, LabAnalysisModule),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Id.Should().Be(WorklistItemId);
        details.SampleId.Should().Be(SampleId);
        details.SampleBarcode.Should().Be(SampleBarcode);
        details.ExamCode.Should().Be(ExamCode);
        details.PatientId.Should().Be(PatientId);
        details.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task RecordAnalysisResultIsSuccessful()
    {
        await LabAnalysisModule.ExecuteCommandAsync(new CreateWorklistItemCommand(
            WorklistItemId, SampleId, SampleBarcode, ExamCode, PatientId, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(WorklistItemId, LabAnalysisModule),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new RecordAnalysisResultCommand(
            WorklistItemId,
            "7.4 mmol/L",
            AnalystId,
            SystemClock.Now
        )).ConfigureAwait(true);

        WorklistItemDetailsDto? details = await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId,
                LabAnalysisModule,
                dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("ResultReceived");
        details.ResultValue.Should().Be("7.4 mmol/L");
    }

    [Fact]
    public async Task GenerateReportIsSuccessful()
    {
        await LabAnalysisModule.ExecuteCommandAsync(new CreateWorklistItemCommand(
            WorklistItemId, SampleId, SampleBarcode, ExamCode, PatientId, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(WorklistItemId, LabAnalysisModule),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new RecordAnalysisResultCommand(
            WorklistItemId, "7.4 mmol/L", AnalystId, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId, LabAnalysisModule, dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new GenerateReportCommand(
            WorklistItemId,
            "/reports/worklist/SC-INT-001.pdf",
            SystemClock.Now
        )).ConfigureAwait(true);

        WorklistItemDetailsDto? details = await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId,
                LabAnalysisModule,
                dto => dto?.Status == "ReportGenerated"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("ReportGenerated");
        details.ReportPath.Should().Be("/reports/worklist/SC-INT-001.pdf");
    }

    [Fact]
    public async Task CompleteWorklistItemIsSuccessful()
    {
        await LabAnalysisModule.ExecuteCommandAsync(new CreateWorklistItemCommand(
            WorklistItemId, SampleId, SampleBarcode, ExamCode, PatientId, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(WorklistItemId, LabAnalysisModule),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new RecordAnalysisResultCommand(
            WorklistItemId, "7.4 mmol/L", AnalystId, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId, LabAnalysisModule, dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new GenerateReportCommand(
            WorklistItemId, "/reports/worklist/SC-INT-001.pdf", SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId, LabAnalysisModule, dto => dto?.Status == "ReportGenerated"),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new CompleteWorklistItemCommand(
            WorklistItemId,
            "Complete",
            SystemClock.Now
        )).ConfigureAwait(true);

        WorklistItemDetailsDto? details = await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId,
                LabAnalysisModule,
                dto => dto?.Status == "Completed"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("Completed");
        details.CompletionType.Should().Be("Complete");
        details.CompletedAt.Should().NotBeNull();
    }
}
