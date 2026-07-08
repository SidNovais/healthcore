namespace HC.LIS.API.Configuration.EventBus;

internal sealed class EventBusOptions
{
    internal const string SectionName = "EventBus";

    public string Type { get; init; } = "rabbitmq";
    public string ConnectionString { get; init; } = "amqp://guest:guest@localhost:5672/";
}
