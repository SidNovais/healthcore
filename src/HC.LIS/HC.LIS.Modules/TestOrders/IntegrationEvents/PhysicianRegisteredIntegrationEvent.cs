using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.TestOrders.IntegrationEvents;

public class PhysicianRegisteredIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid physicianId,
    string fullName,
    string? licenceNumber,
    DateTime registeredAt
) : IntegrationEvent(id, occurredAt)
{
    public Guid PhysicianId { get; } = physicianId;
    public string FullName { get; } = fullName;
    public string? LicenceNumber { get; } = licenceNumber;
    public DateTime RegisteredAt { get; } = registeredAt;
}
