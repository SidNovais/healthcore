using System;
using MediatR;

namespace HC.Core.Domain;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OcurredAt { get; }
}
