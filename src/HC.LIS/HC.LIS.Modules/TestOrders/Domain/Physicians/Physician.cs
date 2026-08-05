using System;
using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Events;
using HC.LIS.Modules.TestOrders.Domain.Physicians.Rules;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians;

public class Physician : AggregateRoot
{
    private PhysicianInfo? _info;
    private PhysicianStatus _status;

    private Physician() { }

    protected override void Apply(IDomainEvent domainEvent) => When((dynamic)domainEvent);

    public static Physician Register(
        Guid id,
        string fullName,
        string? licenceNumber,
        DateTime registeredAt
    )
    {
        CheckRule(new PhysicianMustHaveFullNameRule(fullName));
        Physician physician = new();
        PhysicianRegisteredDomainEvent @event = new(id, fullName, licenceNumber, registeredAt);
        physician.Apply(@event);
        physician.AddDomainEvent(@event);
        return physician;
    }

    public void Update(string fullName, string? licenceNumber, DateTime updatedAt)
    {
        CheckRule(new CannotUpdateInactivePhysicianRule(_status));
        CheckRule(new PhysicianMustHaveFullNameRule(fullName));
        PhysicianUpdatedDomainEvent @event = new(Id, fullName, licenceNumber, updatedAt);
        Apply(@event);
        AddDomainEvent(@event);
    }

    public void Deactivate(DateTime deactivatedAt)
    {
        CheckRule(new CannotDeactivateInactivePhysicianRule(_status));
        PhysicianDeactivatedDomainEvent @event = new(Id, deactivatedAt);
        Apply(@event);
        AddDomainEvent(@event);
    }

    public void Reactivate(DateTime reactivatedAt)
    {
        CheckRule(new CannotReactivateActivePhysicianRule(_status));
        PhysicianReactivatedDomainEvent @event = new(Id, reactivatedAt);
        Apply(@event);
        AddDomainEvent(@event);
    }

    private void When(PhysicianRegisteredDomainEvent @event)
    {
        Id = @event.PhysicianId;
        _info = PhysicianInfo.Of(@event.FullName, @event.LicenceNumber);
        _status = PhysicianStatus.Active;
    }

    private void When(PhysicianUpdatedDomainEvent @event)
    {
        _info = PhysicianInfo.Of(@event.FullName, @event.LicenceNumber);
    }

    private void When(PhysicianDeactivatedDomainEvent @event)
    {
        _status = PhysicianStatus.Inactive;
    }

    private void When(PhysicianReactivatedDomainEvent @event)
    {
        _status = PhysicianStatus.Active;
    }
}
