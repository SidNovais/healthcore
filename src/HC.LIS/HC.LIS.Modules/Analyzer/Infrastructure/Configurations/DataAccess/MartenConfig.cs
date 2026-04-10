using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;
using JasperFx;
using JasperFx.Events;
using Marten;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations.DataAccess;

public static class MartenConfig
{
    public static IDocumentStore BuildDocumentStore(string connectionString)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "analyzer";
            options.AutoCreateSchemaObjects = AutoCreate.None;
            options.Events.StreamIdentity = StreamIdentity.AsString;
            options.Events.AddEventType<AnalyzerSampleCreatedDomainEvent>();
            options.Events.AddEventType<WorklistItemAssignedDomainEvent>();
            options.Events.AddEventType<SampleInfoDispatchedDomainEvent>();
            options.Events.AddEventType<ExamResultReceivedDomainEvent>();
        });
        return store;
    }
}
