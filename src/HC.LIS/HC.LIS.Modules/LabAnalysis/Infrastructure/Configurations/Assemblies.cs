using System.Reflection;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;
  internal static class Assemblies
  {
    public static readonly Assembly Application = typeof(ILabAnalysisModule).Assembly;
    public static readonly Assembly IntegrationEvents = typeof(IntegrationEventsAssemblyInfo).Assembly;
  }
