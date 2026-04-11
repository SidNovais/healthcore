using Autofac;
using HC.LIS.Modules.SampleCollection.Application.Contracts;
using HC.LIS.Modules.SampleCollection.Infrastructure;

namespace HC.LIS.API.Modules.SampleCollection;

internal sealed class SampleCollectionAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SampleCollectionModule>()
            .As<ISampleCollectionModule>()
            .InstancePerLifetimeScope();
    }
}
