using Autofac;
using Serilog;

namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Logging;

internal class LoggingModule(ILogger logger) : Autofac.Module
{
    private readonly ILogger _logger = logger;

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(_logger)
          .As<ILogger>()
          .SingleInstance()
        ;
    }
}
