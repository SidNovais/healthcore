using System.Reflection;
using HC.LIS.Modules.TestOrders.Application.Contracts;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations;
  internal static class Assemblies
  {
    public static readonly Assembly Application = typeof(ITestOrdersModule).Assembly;
    public static readonly Assembly IntegrationEvents = typeof(IntegrationEventsAssemblyInfo).Assembly;
  }
