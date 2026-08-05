using System;
using System.Text.Json;
using FluentAssertions;
using HC.Core.Infrastructure.RealTime;
using HC.LIS.API.Configuration.RealTime;
using HC.LIS.Modules.TestOrders.IntegrationEvents;

namespace HC.LIS.API.Tests.RealTime;

/// <summary>
/// Pins the wire shape of the SSE frames the SPA patches its signals from: a live-inserted row has
/// to carry the same fields a fresh load of the same list would.
/// </summary>
public sealed class UiNotificationTranslatorTests
{
    [Fact]
    public void OrderAddedCarriesTheRequestingPhysicianName()
    {
        var orderId = Guid.NewGuid();
        var physicianId = Guid.NewGuid();

        UiNotification notification = UiNotificationTranslator.OrderAdded(new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            orderId,
            Guid.NewGuid(),
            physicianId,
            "Routine",
            DateTime.UtcNow,
            "Maria Silva",
            "Dr. Ana Lima"));

        JsonElement entity = JsonDocument.Parse(notification.Data).RootElement.GetProperty("entity");
        entity.GetProperty("orderId").GetGuid().Should().Be(orderId);
        entity.GetProperty("requestedBy").GetGuid().Should().Be(physicianId);
        entity.GetProperty("requestedByName").GetString().Should().Be("Dr. Ana Lima");
    }

    [Fact]
    public void OrderAddedEmitsANullPhysicianNameWhenTheEventCarriesNone()
    {
        UiNotification notification = UiNotificationTranslator.OrderAdded(new OrderCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Routine",
            DateTime.UtcNow));

        JsonElement entity = JsonDocument.Parse(notification.Data).RootElement.GetProperty("entity");
        entity.GetProperty("requestedByName").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
