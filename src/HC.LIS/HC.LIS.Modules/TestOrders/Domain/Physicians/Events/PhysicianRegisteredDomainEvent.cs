using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians.Events;

public class PhysicianRegisteredDomainEvent(
    Guid physicianId,
    string fullName,
    string? licenceNumber,
    DateTime registeredAt
) : DomainEvent
{
    public Guid PhysicianId { get; } = physicianId;
    public string FullName { get; } = fullName;
    public string? LicenceNumber { get; } = licenceNumber;
    public DateTime RegisteredAt { get; } = registeredAt;
}
