using System.Reflection;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;
  internal static class Assemblies
  {
    public static readonly Assembly Application = typeof(ISampleCollectionModule).Assembly;
    public static readonly Assembly IntegrationEvents = typeof(IntegrationEventsAssemblyInfo).Assembly;
  }
