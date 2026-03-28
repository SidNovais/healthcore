using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            { "WorklistItemCreatedDomainEvent",      typeof(WorklistItemCreatedDomainEvent) },
            { "AnalysisResultRecordedDomainEvent",   typeof(AnalysisResultRecordedDomainEvent) },
            { "ReportGeneratedDomainEvent",          typeof(ReportGeneratedDomainEvent) },
            { "WorklistItemCompletedDomainEvent",    typeof(WorklistItemCompletedDomainEvent) },
        };
    }
}
