namespace HC.LIS.API.Configuration.EventBus;

internal static class ModuleEventBusFactoryBuilder
{
    internal static async Task<IModuleEventBusFactory> CreateAsync(
        IConfiguration config, Serilog.ILogger logger)
    {
        var options = new EventBusOptions();
        config.GetSection(EventBusOptions.SectionName).Bind(options);

        if (options.Type.Equals("rabbitmq", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(options.ConnectionString);
            logger.Information("Event bus: RabbitMQ at {Host}:{Port}", uri.Host, uri.Port);
            return await RabbitMqModuleEventBusFactory
                .CreateAsync(options, HcLisEventRegistry.Build(), logger).ConfigureAwait(false);
        }

        logger.Information("Event bus: in-memory (integration events will not cross process boundary)");
        return new MemoryModuleEventBusFactory(logger);
    }
}
