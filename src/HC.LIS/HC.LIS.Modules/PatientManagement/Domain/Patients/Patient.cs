using System;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Events;
using HC.LIS.Modules.PatientManagement.Domain.Patients.Rules;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients;

public class Patient : AggregateRoot
{
    private PatientInfo? _info;
    private PatientStatus _status;

    private Patient() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static Patient Register(
        Guid id,
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email,
        DateTime registeredAt
    )
    {
        Patient patient = new();
        PatientRegisteredDomainEvent @event = new(
            id,
            fullName,
            dateOfBirth,
            gender,
            mothersFullName,
            documentId,
            phone,
            email,
            registeredAt
        );
        patient.Apply(@event);
        patient.AddDomainEvent(@event);
        return patient;
    }

    public void Update(
        string fullName,
        DateTime dateOfBirth,
        string? gender,
        string? mothersFullName,
        string? documentId,
        string? phone,
        string? email,
        DateTime updatedAt
    )
    {
        CheckRule(new CannotUpdateAnonymizedPatientRule(_status));
    }

    public void Anonymize(DateTime anonymizedAt)
    {
        CheckRule(new CannotAnonymizeAlreadyAnonymizedPatientRule(_status));
    }

    private void When(PatientRegisteredDomainEvent @event)
    {
        Id = @event.PatientId;
        _info = PatientInfo.Of(
            @event.FullName,
            @event.DateOfBirth,
            @event.Gender,
            @event.MothersFullName,
            @event.DocumentId,
            @event.Phone,
            @event.Email
        );
        _status = PatientStatus.Active;
    }

    private void When(PatientUpdatedDomainEvent @event)
    {
        _info = PatientInfo.Of(@event.FullName, @event.DateOfBirth, @event.Gender, @event.MothersFullName, @event.DocumentId, @event.Phone, @event.Email);
    }

    private void When(PatientAnonymizedDomainEvent @event)
    {
        _info = PatientInfo.Anonymized();
        _status = PatientStatus.Anonymized;
    }
}
