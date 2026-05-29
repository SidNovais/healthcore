using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.AggregateStore;

internal static class DomainEventTypeMappings
{
    internal static IDictionary<string, Type> Dictionary { get; }

    static DomainEventTypeMappings()
    {
        Dictionary = new Dictionary<string, Type>
        {
            { "PatientRegisteredDomainEvent", typeof(PatientRegisteredDomainEvent) },
            { "PatientUpdatedDomainEvent", typeof(PatientUpdatedDomainEvent) },
            { "PatientAnonymizedDomainEvent", typeof(PatientAnonymizedDomainEvent) },
        };
    }
}
