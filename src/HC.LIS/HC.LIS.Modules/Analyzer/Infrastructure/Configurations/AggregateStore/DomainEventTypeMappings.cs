using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            { "AnalyzerSampleCreatedDomainEvent", typeof(AnalyzerSampleCreatedDomainEvent) },
            { "WorklistItemAssignedDomainEvent",  typeof(WorklistItemAssignedDomainEvent)  },
            { "SampleInfoDispatchedDomainEvent",  typeof(SampleInfoDispatchedDomainEvent)  },
            { "ExamResultReceivedDomainEvent",    typeof(ExamResultReceivedDomainEvent)    },
        };
    }
}
