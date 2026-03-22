using System.Threading.Tasks;
using FluentAssertions;
using HC.LIS.Modules.SampleCollection.Application.Collections.AddExamToCollection;
using HC.LIS.Modules.SampleCollection.Application.Collections.CallPatient;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateBarcode;
using HC.LIS.Modules.SampleCollection.Application.Collections.CreateCollectionRequest;
using HC.LIS.Modules.SampleCollection.Application.Collections.MovePatientToWaiting;
using HC.LIS.Modules.SampleCollection.Application.Collections.RecordSampleCollection;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

public class CollectionRequestTests : TestBase
{
    public CollectionRequestTests() : base(System.Guid.CreateVersion7()) { }

    [Fact]
    public async Task CreateCollectionRequestIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);

        var message = await GetLastOutboxMessage<PatientArrivedNotification>().ConfigureAwait(true);

        message.Should().NotBeNull();
        message!.DomainEvent.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
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

        var message = await GetLastOutboxMessage<SampleCreatedForExamNotification>().ConfigureAwait(true);

        message.Should().NotBeNull();
        message!.DomainEvent.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
    }

    [Fact]
    public async Task MovePatientToWaitingIsSuccessful()
    {
        await CollectionRequestFactory.CreateAsync(SampleCollectionModule).ConfigureAwait(true);
        await SampleCollectionModule.ExecuteCommandAsync(
            new MovePatientToWaitingCommand(
                CollectionRequestSampleData.CollectionRequestId,
                CollectionRequestSampleData.WaitingAt)).ConfigureAwait(true);

        var message = await GetLastOutboxMessage<PatientWaitingNotification>().ConfigureAwait(true);

        message.Should().NotBeNull();
        message!.DomainEvent.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
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

        var message = await GetLastOutboxMessage<PatientCalledNotification>().ConfigureAwait(true);

        message.Should().NotBeNull();
        message!.DomainEvent.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
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

        var message = await GetLastOutboxMessage<BarcodeCreatedNotification>().ConfigureAwait(true);

        message.Should().NotBeNull();
        message!.DomainEvent.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
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

        var message = await GetLastOutboxMessage<SampleCollectedNotification>().ConfigureAwait(true);

        message.Should().NotBeNull();
        message!.DomainEvent.CollectionRequestId.Should().Be(CollectionRequestSampleData.CollectionRequestId);
    }
}
