using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.UpdatePatient;

internal class UpdatePatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<UpdatePatientCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        UpdatePatientCommand command,
        CancellationToken cancellationToken
    )
    {
        Patient? patient = await _aggregateStore.Load(new PatientId(command.PatientId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Patient must exist to update");

        patient.Update(
            command.FullName,
            command.DateOfBirth,
            command.Gender,
            command.MothersFullName,
            command.DocumentId,
            command.Phone,
            command.Email,
            command.UpdatedAt
        );

        _aggregateStore.AppendChanges(patient);
    }
}
