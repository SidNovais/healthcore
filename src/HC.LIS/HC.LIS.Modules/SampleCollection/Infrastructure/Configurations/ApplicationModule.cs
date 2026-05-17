using Autofac;
using HC.LIS.Modules.SampleCollection.Application;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Queries;
using HC.LIS.Modules.SampleCollection.Infrastructure.Barcodes;
using HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Processing;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations;

internal class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<QueryExecutor>()
          .As<IQueryExecutor>()
          .InstancePerLifetimeScope()
        ;

        builder.RegisterType<RandomAlphanumericBarcodeValueGenerator>()
          .As<IBarcodeValueGenerator>()
          .InstancePerLifetimeScope()
        ;
    }
}
