using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.API.Configuration.EventBus;

internal interface IModuleBusProvider : IDisposable
{
    IEventsBus TestOrders { get; }
    IEventsBus SampleCollection { get; }
    IEventsBus Analyzer { get; }
    IEventsBus LabAnalysis { get; }
    IEventsBus UserAccess { get; }
    IEventsBus PatientManagement { get; }

    /// <summary>
    /// A dedicated consumer the API uses to observe integration events and relay them to
    /// browser clients over SSE. Under RabbitMQ this is its own <c>hclis.ui_notifications</c>
    /// queue; under the in-memory bus it shares the process-wide singleton.
    /// </summary>
    IEventsBus UiNotifications { get; }

    void StartConsuming();
}
