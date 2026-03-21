using HC.LIS.Modules.SampleCollection.Domain.Collections.Events;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            { "PatientArrivedDomainEvent", typeof(PatientArrivedDomainEvent) },
            { "SampleCreatedForExamDomainEvent", typeof(SampleCreatedForExamDomainEvent) },
            { "ExamAddedToExistingSampleDomainEvent", typeof(ExamAddedToExistingSampleDomainEvent) },
            { "PatientWaitingDomainEvent", typeof(PatientWaitingDomainEvent) },
            { "PatientCalledDomainEvent", typeof(PatientCalledDomainEvent) },
            { "BarcodeCreatedDomainEvent", typeof(BarcodeCreatedDomainEvent) },
            { "SampleCollectedDomainEvent", typeof(SampleCollectedDomainEvent) },
        };
    }
}
