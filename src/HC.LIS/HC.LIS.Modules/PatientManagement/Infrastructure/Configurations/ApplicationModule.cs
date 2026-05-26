using Autofac;
using HC.LIS.Modules.PatientManagement.Application.Configuration.Queries;
using HC.LIS.Modules.PatientManagement.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.PatientManagement.Infrastructure.Configurations;

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
