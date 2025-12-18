using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Orders;

public class OrderItemId(Guid value) : Id(value) { }
