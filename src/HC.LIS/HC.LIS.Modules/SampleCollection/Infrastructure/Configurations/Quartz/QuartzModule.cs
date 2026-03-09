using Autofac;
using Quartz;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.Configurations.Quartz;

public class QuartzModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(x => typeof(IJob).IsAssignableFrom(x)).InstancePerDependency();
    }
}
