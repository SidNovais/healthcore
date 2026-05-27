using HC.Core.Application;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Commands;
using HC.LIS.Modules.PatientManagement.Domain.Patients;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.AnonymizePatient;

internal class AnonymizePatientCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<AnonymizePatientCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public async Task Handle(
        AnonymizePatientCommand command,
        CancellationToken cancellationToken
    )
    {
        Patient? patient = await _aggregateStore.Load(new PatientId(command.PatientId)).ConfigureAwait(false) ??
            throw new InvalidCommandException("Patient must exist to anonymize");

        patient.Anonymize(command.AnonymizedAt);

        _aggregateStore.AppendChanges(patient);
    }
}
