using HC.Core.Domain;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class Order : AggregateRoot
{
    protected override void Apply(IDomainEvent domainEvent) => throw new System.NotImplementedException();
}
