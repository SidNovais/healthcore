using Autofac;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations;

internal static class UserAccessCompositionRoot
{
    private static IContainer s_container = null!;
    public static void SetContainer(IContainer container) => s_container = container;
    public static ILifetimeScope BeginLifetimeScope() => s_container.BeginLifetimeScope();
}
