using System;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Infrastructure;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.Infrastructure;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.Application.Orders.AcceptExam;
using HC.LIS.Modules.TestOrders.Application.Orders.CreateOrder;
using HC.LIS.Modules.TestOrders.Application.Orders.RequestExam;
using HC.LIS.Modules.TestOrders.Infrastructure;
using HC.LIS.Tests.IntegrationEvents.Probes;
using Npgsql;

namespace HC.LIS.Tests.IntegrationEvents.SampleCollection;

[Collection("IntegrationTests")]
public abstract class TestBase : HC.LIS.Tests.IntegrationEvents.TestBase
{
    protected ITestOrdersModule TestOrdersModule { get; private set; } = null!;
    protected ISampleCollectionModule SampleCollectionModule { get; private set; } = null!;
    protected IAnalyzerModule AnalyzerModule { get; private set; } = null!;
    protected ILabAnalysisModule LabAnalysisModule { get; private set; } = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        TestOrdersModule = new TestOrdersModule();
        SampleCollectionModule = new SampleCollectionModule();
        AnalyzerModule = new AnalyzerModule();
        LabAnalysisModule = new LabAnalysisModule();
    }

    public override Task DisposeAsync() => base.DisposeAsync();

    protected async Task<(Guid orderId, Guid orderItemId, Guid sampleId, string barcode)> SetupCollectedSampleAsync(
        string barcode,
        string examMnemonic)
    {
        var patientId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        await TestOrdersModule.ExecuteCommandAsync(new CreateOrderCommand(
            orderId, patientId, ExecutionContext.UserId, "Routine", SystemClock.Now));

        await TestOrdersModule.ExecuteCommandAsync(new RequestExamCommand(
            orderId, orderItemId, examMnemonic, "BLOOD", "Whole Blood",
            "EDTA Tube", "EDTA", "Centrifuge", "Room Temperature", SystemClock.Now));

        await TestOrdersModule.ExecuteCommandAsync(
            new AcceptExamCommand(orderId, orderItemId, SystemClock.Now));

        var crProbe = new GetCollectionRequestFromSampleCollectionProbe(patientId, SampleCollectionModule);
        await IntegrationTestAssert.AssertEventually(crProbe, timeoutMs: 15_000);
        var cr = await crProbe.GetSampleAsync();
        var collectionRequestId = cr!.CollectionRequestId;

        await using var connection = new NpgsqlConnection(ConnectionString);
        var sampleId = await connection.ExecuteScalarAsync<Guid>(
            @"SELECT ""Id"" FROM sample_collection.""SampleDetails"" WHERE ""CollectionRequestId"" = @CollectionRequestId LIMIT 1",
            new { CollectionRequestId = collectionRequestId });

        await SampleCollectionModule.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(collectionRequestId, SystemClock.Now));

        await SampleCollectionModule.ExecuteCommandAsync(
            new CreateBarcodeCommand(collectionRequestId, "EDTA Tube", barcode, ExecutionContext.UserId, SystemClock.Now));

        await SampleCollectionModule.ExecuteCommandAsync(
            new CallPatientCommand(collectionRequestId, ExecutionContext.UserId, SystemClock.Now));

        await SampleCollectionModule.ExecuteCommandAsync(
            new RecordSampleCollectionCommand(
                collectionRequestId,
                sampleId,
                ExecutionContext.UserId,
                "Test Patient",
                new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                "M",
                SystemClock.Now));

        return (orderId, orderItemId, sampleId, barcode);
    }
}
