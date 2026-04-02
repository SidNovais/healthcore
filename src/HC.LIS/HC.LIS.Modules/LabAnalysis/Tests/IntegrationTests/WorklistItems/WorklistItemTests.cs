using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FluentAssertions;
using Newtonsoft.Json;
using Npgsql;
using HC.Core.Domain;
using HC.Core.Infrastructure.Serialization;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CompleteWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.IntegrationTests.WorklistItems;

public class WorklistItemTests : TestBase
{
    private static readonly Guid WorklistItemId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95f01");
    private static readonly Guid SampleId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550e0");
    private const string SampleBarcode = "SC-INT-001";
    private const string ExamCode = "019b6c5d-fbf9-7e35-aa12-c38922ec5040";
    private static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550f0");
    private static readonly Guid PerformedById = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5041");
    private const string AnalyteCode = "GLU";
    private const string ResultValue = "7.4";
    private const string ResultUnit = "mmol/L";
    private const string ReferenceRange = "3.5-5.5 mmol/L";

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
            AnalyteCode,
            ResultValue,
            ResultUnit,
            ReferenceRange,
            PerformedById,
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
        AnalyteResultDto result = details.AnalyteResults.Should().ContainSingle().Subject;
        result.AnalyteCode.Should().Be(AnalyteCode);
        result.ResultValue.Should().Be(ResultValue);
        result.ResultUnit.Should().Be(ResultUnit);
        result.ReferenceRange.Should().Be(ReferenceRange);
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
            WorklistItemId, AnalyteCode, ResultValue, ResultUnit, ReferenceRange, PerformedById, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId, LabAnalysisModule, dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        WorklistItemDetailsDto? details = await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId,
                LabAnalysisModule,
                dto => dto?.Status == "ReportGenerated"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("ReportGenerated");
        details.ReportPath.Should().Be($"/reports/worklist/{WorklistItemId}.pdf");
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
            WorklistItemId, AnalyteCode, ResultValue, ResultUnit, ReferenceRange, PerformedById, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId, LabAnalysisModule, dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId, LabAnalysisModule, dto => dto?.Status == "ReportGenerated"),
            15000
        ).ConfigureAwait(true);

        await LabAnalysisModule.ExecuteCommandAsync(new CompleteWorklistItemCommand(
            WorklistItemId,
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

    [Fact]
    public async Task RecordAnalysisResultViaAnalyzerEventIsSuccessful()
    {
        await LabAnalysisModule.ExecuteCommandAsync(new CreateWorklistItemCommand(
            WorklistItemId, SampleId, SampleBarcode, ExamCode, PatientId, SystemClock.Now
        )).ConfigureAwait(true);

        await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(WorklistItemId, LabAnalysisModule),
            15000
        ).ConfigureAwait(true);

        var integrationEvent = new AnalyzerResultReceivedIntegrationEvent(
            Guid.CreateVersion7(),
            SystemClock.Now,
            WorklistItemId,
            PerformedById,
            AnalyteCode,
            ResultValue,
            ResultUnit,
            ReferenceRange,
            SystemClock.Now
        );

        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            string? type = integrationEvent.GetType().FullName;
            string data = JsonConvert.SerializeObject(integrationEvent, new JsonSerializerSettings
            {
                ContractResolver = new AllPropertiesContractResolver()
            });
            await connection.ExecuteScalarAsync(
                @"INSERT INTO ""lab_analysis"".""InboxMessages"" (""Id"", ""OccurredAt"", ""Type"", ""Data"") VALUES (@Id, @OccurredAt, @Type, @Data)",
                new { integrationEvent.Id, integrationEvent.OccurredAt, type, data }
            ).ConfigureAwait(true);
        }

        WorklistItemDetailsDto? details = await GetEventually(
            new GetWorklistItemDetailsFromLabAnalysisProbe(
                WorklistItemId,
                LabAnalysisModule,
                dto => dto?.Status == "ResultReceived"),
            15000
        ).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be("ResultReceived");
        AnalyteResultDto result = details.AnalyteResults.Should().ContainSingle().Subject;
        result.AnalyteCode.Should().Be(AnalyteCode);
        result.ResultValue.Should().Be(ResultValue);
        result.ResultUnit.Should().Be(ResultUnit);
        result.ReferenceRange.Should().Be(ReferenceRange);
    }
}
