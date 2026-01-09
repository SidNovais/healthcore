using Autofac;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations;

internal static class TestOrdersCompositionRoot
{
    private static IContainer s_container;
    public static void SetContainer(IContainer container) => s_container = container;
    public static ILifetimeScope BeginLifetimeScope() => s_container.BeginLifetimeScope();
}
