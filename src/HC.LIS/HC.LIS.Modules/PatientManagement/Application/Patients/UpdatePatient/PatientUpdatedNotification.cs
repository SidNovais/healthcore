using HC.Core.Application.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

public class PatientUpdatedNotification(PatientUpdatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientUpdatedDomainEvent>(domainEvent, id)
{

}
