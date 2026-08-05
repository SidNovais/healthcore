using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.TestOrders.Domain.Physicians;

public class PhysicianId(Guid value) : AggregateId<Physician>(value);
