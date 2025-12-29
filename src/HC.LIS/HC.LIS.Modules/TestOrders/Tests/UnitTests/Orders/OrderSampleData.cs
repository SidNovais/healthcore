using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.UnitTests.Orders;

public readonly struct OrderSampleData
{
    public static readonly Guid OrderId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e40");
    public static readonly Guid OrderItemId = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5026");
    public static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550c0");
    public static readonly Guid RequestedBy = Guid.Parse("019b664c-79f0-7f45-87f3-84664a00e635");
    public static readonly string OrderPriority = "Routine";
    public static readonly string SpecimenMnemonic = "BLD";
    public static readonly string MaterialType = "Whole Blood";
    public static readonly string ContainerType = "Vacutainer Tube";
    public static readonly string Additive = "None";
    public static readonly string ProcessingType = "None";
    public static readonly string StorageCondition = "Room Temperature";
    public static readonly DateTime RequestedAt = SystemClock.Now;
}
