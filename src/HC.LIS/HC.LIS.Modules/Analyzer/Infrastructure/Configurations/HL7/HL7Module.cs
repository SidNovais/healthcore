using Autofac;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;
using HC.LIS.Modules.Analyzer.Application.Contracts;
using HC.LIS.Modules.Analyzer.Infrastructure.HL7;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Configurations.HL7;

internal class HL7Module(bool enableHl7Checksum) : Autofac.Module
{
    private readonly bool _enableHl7Checksum = enableHl7Checksum;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<HL7SampleInfoPresenter>()
            .As<ISampleInfoPresenter>()
            .SingleInstance();

        builder.Register(_ => new HL7ResultParser(_enableHl7Checksum))
            .As<IHL7ResultParser>()
            .SingleInstance();

        builder.Register(_ => new HL7QueryParser(_enableHl7Checksum))
            .As<IHL7QueryParser>()
            .SingleInstance();
    }
}
