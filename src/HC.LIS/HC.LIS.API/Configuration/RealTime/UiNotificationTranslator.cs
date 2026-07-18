using System.Text.Json;
using HC.Core.Infrastructure.EventBus;
using HC.Core.Infrastructure.RealTime;
using HC.LIS.Modules.SampleCollection.IntegrationEvents;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.API.Configuration.RealTime;

/// <summary>
/// Maps integration events into browser-ready <see cref="UiNotification"/>s and wires the UI
/// consumer bus to relay them. Each mapping emits the minimal op the client needs (see the SSE
/// contract): a status transition patches only the badge; a creation carries the full row.
/// </summary>
internal static class UiNotificationTranslator
{
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    internal static void Subscribe(IEventsBus bus, IUiNotificationHub hub)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(hub);

        // ── Orders: exam-item status transitions patch the order-detail row's badge ──────────
        Relay<OrderItemAcceptedIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "Accepted"));
        Relay<OrderItemCanceledIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "Canceled"));
        Relay<OrderItemRejectedIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "Rejected"));
        Relay<OrderItemPlacedOnHoldIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "OnHold"));
        Relay<OrderItemPlacedInProgressIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "InProgress"));
        Relay<OrderItemCompletedIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "Completed"));
        Relay<OrderItemPartiallyCompletedIntegrationEvent>(bus, hub, e => ExamStatus(e.OrderItemId, "PartiallyCompleted"));

        // ── Triage: collection-request queue transitions (arrived → waiting → called → done) ──
        Relay<PatientArrivedIntegrationEvent>(bus, hub, e => TriageAdd(e.CollectionRequestId, e.PatientId, e.OccurredAt));
        Relay<PatientWaitingIntegrationEvent>(bus, hub, e => TriageMove(e.CollectionRequestId, "waiting", "Waiting"));
        Relay<PatientCalledIntegrationEvent>(bus, hub, e => TriageMove(e.CollectionRequestId, "called", "Called"));
        Relay<SampleCollectedIntegrationEvent>(bus, hub, e => TriageRemove(e.CollectionRequestId));
    }

    private static void Relay<TEvent>(IEventsBus bus, IUiNotificationHub hub, Func<TEvent, UiNotification?> map)
        where TEvent : IntegrationEvent =>
        bus.Subscribe(new UiNotificationListener<TEvent>(hub, map));

    // ── Payload builders ─────────────────────────────────────────────────────────────────────

    internal static UiNotification ExamStatus(Guid orderItemId, string status) =>
        new(UiTopics.Orders, Serialize(new { op = "status", scope = "exam", orderItemId, status }));

    internal static UiNotification TriageAdd(Guid collectionRequestId, Guid patientId, DateTime arrivedAt) =>
        new(UiTopics.Triage, Serialize(new
        {
            op = "add",
            queue = "arrived",
            entity = new { collectionRequestId, patientId, status = "Arrived", arrivedAt },
        }));

    internal static UiNotification TriageMove(Guid collectionRequestId, string queue, string status) =>
        new(UiTopics.Triage, Serialize(new { op = "move", queue, collectionRequestId, status }));

    internal static UiNotification TriageRemove(Guid collectionRequestId) =>
        new(UiTopics.Triage, Serialize(new { op = "remove", collectionRequestId }));

    private static string Serialize(object payload) => JsonSerializer.Serialize(payload, s_json);
}
