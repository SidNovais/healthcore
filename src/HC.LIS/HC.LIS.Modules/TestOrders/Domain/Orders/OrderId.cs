using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class OrderId(Guid id) : AggregateId<Order>(id);
