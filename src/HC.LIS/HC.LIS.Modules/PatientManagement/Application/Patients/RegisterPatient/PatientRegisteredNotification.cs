using HC.Core.Application.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

public class PatientRegisteredNotification(PatientRegisteredDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<PatientRegisteredDomainEvent>(domainEvent, id)
{

}
