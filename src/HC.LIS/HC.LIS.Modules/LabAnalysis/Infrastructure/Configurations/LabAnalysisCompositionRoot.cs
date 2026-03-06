using Autofac;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations;

internal static class LabAnalysisCompositionRoot
{
    private static IContainer s_container = null!;
    public static void SetContainer(IContainer container) => s_container = container;
    public static ILifetimeScope BeginLifetimeScope() => s_container.BeginLifetimeScope();
}
