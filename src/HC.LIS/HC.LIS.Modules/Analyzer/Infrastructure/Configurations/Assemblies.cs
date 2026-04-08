using System.Reflection;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.IntegrationEvents;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations;
  internal static class Assemblies
  {
    public static readonly Assembly Application = typeof(IAnalyzerModule).Assembly;
    public static readonly Assembly IntegrationEvents = typeof(IntegrationEventsAssemblyInfo).Assembly;
  }
