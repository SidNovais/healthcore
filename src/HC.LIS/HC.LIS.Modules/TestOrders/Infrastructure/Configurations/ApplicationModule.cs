using Autofac;
using HC.LIS.Modules.TestOrders.Application.Configuration.Queries;
using HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations;

internal class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<QueryExecutor>()
          .As<IQueryExecutor>()
          .InstancePerLifetimeScope()
        ;
    }
}
