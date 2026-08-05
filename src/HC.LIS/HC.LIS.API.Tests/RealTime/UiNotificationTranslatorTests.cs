using System;
using System.Text.Json;
using FluentAssertions;
using HC.Core.Infrastructure.RealTime;
using HC.LIS.API.Configuration.RealTime;
using HC.LIS.Modules.LabAnalysis.IntegrationEvents;
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

    [Fact]
    public void WorklistAddCarriesTheRequestingPhysicianName()
    {
        var worklistItemId = Guid.NewGuid();

        UiNotification notification = UiNotificationTranslator.WorklistAdd(new WorklistItemCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            worklistItemId,
            Guid.NewGuid(),
            "BC-1",
            "HGB",
            "Maria Silva",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "Female",
            "Ana Lima"));

        JsonElement entity = JsonDocument.Parse(notification.Data).RootElement.GetProperty("entity");
        entity.GetProperty("id").GetGuid().Should().Be(worklistItemId);
        entity.GetProperty("requestedByName").GetString().Should().Be("Ana Lima");
    }

    [Fact]
    public void WorklistAddEmitsANullPhysicianNameWhenTheMappingHasNotArrivedYet()
    {
        UiNotification notification = UiNotificationTranslator.WorklistAdd(new WorklistItemCreatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BC-1",
            "HGB"));

        JsonElement entity = JsonDocument.Parse(notification.Data).RootElement.GetProperty("entity");
        entity.GetProperty("requestedByName").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
