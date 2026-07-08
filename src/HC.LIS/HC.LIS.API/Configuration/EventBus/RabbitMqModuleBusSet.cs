using HC.Core.Infrastructure.EventBus;
using RabbitMQ.Client;

namespace HC.LIS.API.Configuration.EventBus;

internal sealed class RabbitMqModuleBusSet : IDisposable
{
    private readonly RabbitMqEventBus _testOrders;
    private readonly RabbitMqEventBus _sampleCollection;
    private readonly RabbitMqEventBus _analyzer;
    private readonly RabbitMqEventBus _labAnalysis;
    private readonly RabbitMqEventBus _userAccess;
    private readonly RabbitMqEventBus _patientManagement;
    private bool _disposed;

    internal IEventsBus TestOrders => _testOrders;
    internal IEventsBus SampleCollection => _sampleCollection;
    internal IEventsBus Analyzer => _analyzer;
    internal IEventsBus LabAnalysis => _labAnalysis;
    internal IEventsBus UserAccess => _userAccess;
    internal IEventsBus PatientManagement => _patientManagement;

    private RabbitMqModuleBusSet(
        RabbitMqEventBus testOrders,
        RabbitMqEventBus sampleCollection,
        RabbitMqEventBus analyzer,
        RabbitMqEventBus labAnalysis,
        RabbitMqEventBus userAccess,
        RabbitMqEventBus patientManagement)
    {
        _testOrders = testOrders;
        _sampleCollection = sampleCollection;
        _analyzer = analyzer;
        _labAnalysis = labAnalysis;
        _userAccess = userAccess;
        _patientManagement = patientManagement;
    }

    internal static async Task<RabbitMqModuleBusSet> CreateAsync(
        IConnection connection, EventRegistry registry, Serilog.ILogger logger)
    {
        var testOrders = await RabbitMqEventBus.CreateAsync(
            connection, "orders.events", "hclis.test_orders", registry, logger).ConfigureAwait(false);
        var sampleCollection = await RabbitMqEventBus.CreateAsync(
            connection, "sample_collection.events", "hclis.sample_collection", registry, logger).ConfigureAwait(false);
        var analyzer = await RabbitMqEventBus.CreateAsync(
            connection, "analyzer.events", "hclis.analyzer", registry, logger).ConfigureAwait(false);
        var labAnalysis = await RabbitMqEventBus.CreateAsync(
            connection, "lab_analysis.events", "hclis.lab_analysis", registry, logger).ConfigureAwait(false);
        var userAccess = await RabbitMqEventBus.CreateAsync(
            connection, "user_access.events", "hclis.user_access", registry, logger).ConfigureAwait(false);
        var patientManagement = await RabbitMqEventBus.CreateAsync(
            connection, "patient_management.events", "hclis.patient_management", registry, logger).ConfigureAwait(false);

        return new RabbitMqModuleBusSet(testOrders, sampleCollection, analyzer, labAnalysis, userAccess, patientManagement);
    }

    internal void StartConsuming()
    {
        _testOrders.StartConsuming();
        _sampleCollection.StartConsuming();
        _analyzer.StartConsuming();
        _labAnalysis.StartConsuming();
        _userAccess.StartConsuming();
        _patientManagement.StartConsuming();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _testOrders.Dispose();
        _sampleCollection.Dispose();
        _analyzer.Dispose();
        _labAnalysis.Dispose();
        _userAccess.Dispose();
        _patientManagement.Dispose();
    }
}
