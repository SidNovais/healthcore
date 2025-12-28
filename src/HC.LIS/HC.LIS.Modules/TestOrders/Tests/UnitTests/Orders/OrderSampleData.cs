using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.UnitTests.Orders;

public readonly struct OrderSampleData
{
    public static readonly Guid OrderId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e40");
    public static readonly string SpecimenMnemonic = "BLD";
    public static readonly string MaterialType = "Whole Blood";
    public static readonly string ContainerType = "Vacutainer Tube";
    public static readonly string Additive = "None";
    public static readonly string ProcessingType = "None";
    public static readonly string StorageCondition = "Room Temperature";
    public static readonly DateTime RequestedAt = SystemClock.Now;
}
