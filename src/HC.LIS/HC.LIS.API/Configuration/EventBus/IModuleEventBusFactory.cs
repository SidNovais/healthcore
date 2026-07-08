using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.API.Configuration.EventBus;

internal interface IModuleEventBusFactory : IDisposable
{
    IEventsBus TestOrders { get; }
    IEventsBus SampleCollection { get; }
    IEventsBus Analyzer { get; }
    IEventsBus LabAnalysis { get; }
    IEventsBus UserAccess { get; }
    IEventsBus PatientManagement { get; }

    void StartConsuming();
}
