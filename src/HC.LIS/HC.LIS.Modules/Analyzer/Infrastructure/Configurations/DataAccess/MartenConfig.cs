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
            // Register domain event types here, e.g.:
            // options.Events.AddEventType<MyDomainEvent>();
        });
        return store;
    }
}
