using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.API.Configuration.EventBus;

internal sealed class MemoryModuleBusProvider : IModuleBusProvider
{
    public IEventsBus TestOrders { get; }
    public IEventsBus SampleCollection { get; }
    public IEventsBus Analyzer { get; }
    public IEventsBus LabAnalysis { get; }
    public IEventsBus UserAccess { get; }
    public IEventsBus PatientManagement { get; }

    internal MemoryModuleBusProvider(Serilog.ILogger logger)
    {
        TestOrders = new InMemoryEventBusClient(logger);
        SampleCollection = new InMemoryEventBusClient(logger);
        Analyzer = new InMemoryEventBusClient(logger);
        LabAnalysis = new InMemoryEventBusClient(logger);
        UserAccess = new InMemoryEventBusClient(logger);
        PatientManagement = new InMemoryEventBusClient(logger);
    }

    public void StartConsuming() { }

    public void Dispose()
    {
        TestOrders.Dispose();
        SampleCollection.Dispose();
        Analyzer.Dispose();
        LabAnalysis.Dispose();
        UserAccess.Dispose();
        PatientManagement.Dispose();
    }
}
