using System;
using System.Text.Json.Serialization;
using HC.LIS.Modules.LabAnalysis.Application.Configuration.Commands;

namespace HC.LIS.Modules.LabAnalysis.Application.Patients.AnonymizePatientSnapshot;

[method: JsonConstructor]
public class AnonymizePatientSnapshotByPatientIdCommand(
    Guid id,
    Guid patientId,
    DateTime anonymizedAt
) : InternalCommandBase(id)
{
    public Guid PatientId { get; } = patientId;
    public DateTime AnonymizedAt { get; } = anonymizedAt;
}
