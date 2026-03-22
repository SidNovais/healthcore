using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.SampleCollection.Application.Collections.AddExamToCollection;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;
using HC.LIS.Modules.SampleCollection.Domain.Collections;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

public class CollectionRequestTests : TestBase
{
    public CollectionRequestTests() : base(System.Guid.CreateVersion7()) { }

    [Fact]
    public async Task CreateCollectionRequestIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);

        var details = await GetEventually(
            new GetCollectionRequestDetailsFromSampleCollectionProbe(
                CollectionRequestSampleData.CollectionRequestId,
                SampleCollectionModule),
            15000).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        details.PatientId.Should().Be(CollectionRequestSampleData.PatientId);
        details.OrderId.Should().Be(CollectionRequestSampleData.OrderId);
        details.Status.Should().Be(CollectionStatus.Arrived.Value);
        details.ArrivedAt.Should().Be(CollectionRequestSampleData.ArrivedAt);
    }

    [Fact]
    public async Task AddExamToCollectionIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new AddExamToCollectionCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.ExamId,
                CollectionRequestSampleData.TubeType)).ConfigureAwait(true);

        var sampleNotification = await GetLastOutboxMessage<SampleCreatedForExamNotification>().ConfigureAwait(true);
        var sampleId = sampleNotification!.DomainEvent.SampleId;

        var details = await GetEventually(
            new GetSampleDetailsFromSampleCollectionProbe(sampleId, SampleCollectionModule),
            15000).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.SampleId.Should().Be(sampleId);
        details.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
        details.TubeType.Should().Be(CollectionRequestSampleData.TubeType);
        details.Status.Should().Be(SampleStatus.Pending.Value);
    }

    [Fact]
    public async Task MovePatientToWaitingIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.WaitingAt)).ConfigureAwait(true);

        var details = await GetEventually(
            new GetCollectionRequestDetailsFromSampleCollectionProbe(
                CollectionRequestSampleData.CollectionRequestId,
                SampleCollectionModule),
            15000).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be(CollectionStatus.Waiting.Value);
        details.WaitingAt.Should().Be(CollectionRequestSampleData.WaitingAt);
    }

    [Fact]
    public async Task CallPatientIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.WaitingAt)).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new CallPatientCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.TechnicianId,
                CollectionRequestSampleData.CalledAt)).ConfigureAwait(true);

        var details = await GetEventually(
            new GetCollectionRequestDetailsFromSampleCollectionProbe(
                CollectionRequestSampleData.CollectionRequestId,
                SampleCollectionModule),
            15000).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be(CollectionStatus.Called.Value);
        details.CalledAt.Should().Be(CollectionRequestSampleData.CalledAt);
    }

    [Fact]
    public async Task CreateBarcodeIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new AddExamToCollectionCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.ExamId,
                CollectionRequestSampleData.TubeType)).ConfigureAwait(true);

        var sampleNotification = await GetLastOutboxMessage<SampleCreatedForExamNotification>().ConfigureAwait(true);
        var sampleId = sampleNotification!.DomainEvent.SampleId;

        await SampleCollectionModule.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.WaitingAt)).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new CreateBarcodeCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.TubeType,
                CollectionRequestSampleData.BarcodeValue,
                CollectionRequestSampleData.TechnicianId,
                CollectionRequestSampleData.BarcodeCreatedAt)).ConfigureAwait(true);

        var details = await GetEventually(
            new GetSampleDetailsFromSampleCollectionProbe(sampleId, SampleCollectionModule),
            15000).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Barcode.Should().Be(CollectionRequestSampleData.BarcodeValue);
        details.Status.Should().Be(SampleStatus.BarcodeCreated.Value);
    }

    [Fact]
    public async Task RecordSampleCollectionIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new AddExamToCollectionCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.ExamId,
                CollectionRequestSampleData.TubeType)).ConfigureAwait(true);

        var sampleNotification = await GetLastOutboxMessage<SampleCreatedForExamNotification>().ConfigureAwait(true);
        var sampleId = sampleNotification!.DomainEvent.SampleId;

        await SampleCollectionModule.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.WaitingAt)).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new CreateBarcodeCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.TubeType,
                CollectionRequestSampleData.BarcodeValue,
                CollectionRequestSampleData.TechnicianId,
                CollectionRequestSampleData.BarcodeCreatedAt)).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new CallPatientCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.TechnicianId,
                CollectionRequestSampleData.CalledAt)).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new RecordSampleCollectionCommand(
                CollectionRequestSampleData.CollectionRequestId,
                sampleId,
                CollectionRequestSampleData.TechnicianId,
                CollectionRequestSampleData.CollectedAt)).ConfigureAwait(true);

        var details = await GetEventually(
            new GetSampleDetailsFromSampleCollectionProbe(sampleId, SampleCollectionModule),
            15000).ConfigureAwait(true);

        details.Should().NotBeNull();
        details!.Status.Should().Be(SampleStatus.Collected.Value);
        details.CollectedAt.Should().Be(CollectionRequestSampleData.CollectedAt);
    }
}
