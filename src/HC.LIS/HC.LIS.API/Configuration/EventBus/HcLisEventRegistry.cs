using HC.Core.Infrastructure.EventBus;
using HC.LIS.Modules.Analyzer.IntegrationEvents;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;
using HC.LIS.Modules.PatientManagement.IntegrationEvents;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.API.Configuration.EventBus;

internal static class HcLisEventRegistry
{
    internal static EventRegistry Build() =>
        new EventRegistry.RegistryBuilder()
            .Register<OrderItemAcceptedIntegrationEvent>("orders.events", "order_item.accepted")
            .Register<OrderItemRequestedIntegrationEvent>("orders.events", "order_item.requested")
            .Register<OrderItemCanceledIntegrationEvent>("orders.events", "order_item.cancelled")
            .Register<OrderItemRejectedIntegrationEvent>("orders.events", "order_item.rejected")
            .Register<SampleCollectedIntegrationEvent>("sample_collection.events", "sample.collected")
            .Register<ExamResultReceivedIntegrationEvent>("analyzer.events", "exam_result.received")
            .Register<WorklistItemCreatedIntegrationEvent>("lab_analysis.events", "worklist_item.created")
            .Register<WorklistItemCompletedIntegrationEvent>("lab_analysis.events", "worklist_item.completed")
            .Register<PatientRegisteredIntegrationEvent>("patient_management.events", "patient.registered")
            .Register<PatientUpdatedIntegrationEvent>("patient_management.events", "patient.updated")
            .Register<PatientAnonymizedIntegrationEvent>("patient_management.events", "patient.anonymized")
            .Build();
}
