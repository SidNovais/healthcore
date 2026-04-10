using Autofac;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure.HL7;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations.HL7;

internal class HL7Module : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<HL7SampleInfoPresenter>()
            .As<ISampleInfoPresenter>()
            .SingleInstance();

        builder.RegisterType<HL7ResultParser>()
            .As<IHL7ResultParser>()
            .SingleInstance();
    }
}
