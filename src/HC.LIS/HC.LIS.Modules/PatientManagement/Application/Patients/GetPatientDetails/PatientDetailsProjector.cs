using Dapper;
using HC.Core.Application.Projections;
using HC.Core.Domain;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;

namespace HC.LIS.Modules.PatientManagement.Application.Patients.GetPatientDetails;

internal class PatientDetailsProjector(
    ISqlConnectionFactory sqlConnectionFactory
) : ProjectorBase, IProjector
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task Project(IDomainEvent @event)
    {
        await When((dynamic)@event);
    }

    private async Task When(PatientRegisteredDomainEvent patientRegistered)
    {
        string status = "Active";
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"INSERT INTO patient_management.""PatientDetails""
              (""Id"", ""FullName"", ""DateOfBirth"", ""Gender"", ""MothersFullName"", ""DocumentId"", ""Phone"", ""Email"", ""Status"", ""RegisteredAt"")
              VALUES (@PatientId, @FullName, @DateOfBirth, @Gender, @MothersFullName, @DocumentId, @Phone, @Email, @Status, @RegisteredAt)",
            new
            {
                patientRegistered.PatientId,
                patientRegistered.FullName,
                patientRegistered.DateOfBirth,
                patientRegistered.Gender,
                patientRegistered.MothersFullName,
                patientRegistered.DocumentId,
                patientRegistered.Phone,
                patientRegistered.Email,
                Status = status,
                patientRegistered.RegisteredAt
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PatientUpdatedDomainEvent patientUpdated)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE patient_management.""PatientDetails""
              SET ""FullName"" = @FullName,
                  ""DateOfBirth"" = @DateOfBirth,
                  ""Gender"" = @Gender,
                  ""MothersFullName"" = @MothersFullName,
                  ""DocumentId"" = @DocumentId,
                  ""Phone"" = @Phone,
                  ""Email"" = @Email
              WHERE ""Id"" = @PatientId",
            new
            {
                patientUpdated.PatientId,
                patientUpdated.FullName,
                patientUpdated.DateOfBirth,
                patientUpdated.Gender,
                patientUpdated.MothersFullName,
                patientUpdated.DocumentId,
                patientUpdated.Phone,
                patientUpdated.Email
            }
        ).ConfigureAwait(false);
    }

    private async Task When(PatientAnonymizedDomainEvent patientAnonymized)
    {
        using var connection = _sqlConnectionFactory.CreateConnection();
        await connection.ExecuteScalarAsync(
            @"UPDATE patient_management.""PatientDetails""
              SET ""Status"" = 'Anonymized',
                  ""FullName"" = 'ANONYMIZED',
                  ""DateOfBirth"" = '1900-01-01',
                  ""Gender"" = 'ANONYMIZED',
                  ""MothersFullName"" = 'ANONYMIZED',
                  ""DocumentId"" = 'ANONYMIZED',
                  ""Phone"" = 'ANONYMIZED',
                  ""Email"" = 'ANONYMIZED',
                  ""AnonymizedAt"" = @AnonymizedAt
              WHERE ""Id"" = @PatientId",
            new
            {
                patientAnonymized.PatientId,
                patientAnonymized.AnonymizedAt
            }
        ).ConfigureAwait(false);
    }

    private static new Task When(IDomainEvent _) => Task.CompletedTask;
}
