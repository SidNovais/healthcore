using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.RegisterPatient;

internal class RegisterPatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<RegisterPatientCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task<Guid> Handle(
        RegisterPatientCommand command,
        CancellationToken cancellationToken
    )
    {
        Patient patient = Patient.Register(
            command.PatientId,
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email,
            command.RegisteredAt
        );
        _aggregateStore.Start(patient);
        return patient.Id;
    }
}
