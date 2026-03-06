namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            // Register domain event type mappings here, e.g.:
            // { "MyDomainEvent", typeof(MyDomainEvent) },
        };
    }
}
