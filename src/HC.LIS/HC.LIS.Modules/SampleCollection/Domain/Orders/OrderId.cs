using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Orders;

public class OrderId(Guid value) : Id(value);
