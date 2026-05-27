using HC.Core.Application.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

public class PatientAnonymizedNotification(PatientAnonymizedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientAnonymizedDomainEvent>(domainEvent, id)
{

}
