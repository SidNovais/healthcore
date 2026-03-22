using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;
using JasperFx;
using JasperFx.Events;
using Marten;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.DataAccess;

public static class MartenConfig
{
    public static IDocumentStore BuildDocumentStore(string connectionString)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "sample_collection";
            options.AutoCreateSchemaObjects = AutoCreate.None;
            options.Events.StreamIdentity = StreamIdentity.AsString;
            options.Events.AddEventType<PatientArrivedDomainEvent>();
            options.Events.AddEventType<SampleCreatedForExamDomainEvent>();
            options.Events.AddEventType<ExamAddedToExistingSampleDomainEvent>();
            options.Events.AddEventType<PatientWaitingDomainEvent>();
            options.Events.AddEventType<PatientCalledDomainEvent>();
            options.Events.AddEventType<BarcodeCreatedDomainEvent>();
            options.Events.AddEventType<SampleCollectedDomainEvent>();
        });
        return store;
    }
}
