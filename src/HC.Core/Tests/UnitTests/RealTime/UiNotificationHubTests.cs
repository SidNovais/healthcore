using System.Linq;
using FluentAssertions;
using HC.Core.Infrastructure.RealTime;

namespace HC.Core.UnitTests.RealTime;

public sealed class UiNotificationHubTests
{
    private static UiNotification Note(string topic, string data = "{}") => new(topic, data);

    private static IUiNotificationSubscription SubscribeTo(UiNotificationHub hub, params string[] topics) =>
        hub.Subscribe(topics);

    [Fact]
    public void PublishDeliversNotificationToSubscriberOfMatchingTopic()
    {
        var sut = new UiNotificationHub();
        using var subscription = SubscribeTo(sut, "orders");

        sut.Publish(Note("orders", "{\"op\":\"status\"}"));

        subscription.Reader.TryRead(out var received).Should().BeTrue();
        received!.Topic.Should().Be("orders");
        received.Data.Should().Be("{\"op\":\"status\"}");
    }

    [Fact]
    public void PublishSkipsSubscribersNotInterestedInTheTopic()
    {
        var sut = new UiNotificationHub();
        using var subscription = SubscribeTo(sut, "triage");

        sut.Publish(Note("orders"));

        subscription.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public void PublishFansOutToEverySubscriberOfTheTopic()
    {
        var sut = new UiNotificationHub();
        using var first = SubscribeTo(sut, "worklist");
        using var second = SubscribeTo(sut, "worklist");

        sut.Publish(Note("worklist"));

        first.Reader.TryRead(out _).Should().BeTrue();
        second.Reader.TryRead(out _).Should().BeTrue();
    }

    [Fact]
    public void DisposedSubscriptionStopsReceivingAndDoesNotAffectOthers()
    {
        var sut = new UiNotificationHub();
        var gone = SubscribeTo(sut, "orders");
        using var remaining = SubscribeTo(sut, "orders");

        gone.Dispose();
        sut.Publish(Note("orders"));

        gone.Reader.TryRead(out _).Should().BeFalse();
        remaining.Reader.TryRead(out _).Should().BeTrue();
    }

    [Fact]
    public void SubscriberReceivesOnlyItsAllowedTopicsWhenSubscribedToSeveral()
    {
        var sut = new UiNotificationHub();
        using var subscription = SubscribeTo(sut, "orders", "worklist");

        sut.Publish(Note("orders"));
        sut.Publish(Note("triage"));
        sut.Publish(Note("worklist"));

        var topics = Drain(subscription).Select(n => n.Topic).ToList();
        topics.Should().Equal("orders", "worklist");
    }

    [Fact]
    public void BoundedChannelDropsOldestWhenSubscriberDoesNotDrain()
    {
        var sut = new UiNotificationHub(capacityPerSubscriber: 2);
        using var subscription = SubscribeTo(sut, "orders");

        sut.Publish(Note("orders", "1"));
        sut.Publish(Note("orders", "2"));
        sut.Publish(Note("orders", "3"));

        var data = Drain(subscription).Select(n => n.Data).ToList();
        data.Should().Equal("2", "3");
    }

    private static System.Collections.Generic.List<UiNotification> Drain(IUiNotificationSubscription subscription)
    {
        var items = new System.Collections.Generic.List<UiNotification>();
        while (subscription.Reader.TryRead(out var item))
            items.Add(item);
        return items;
    }
}
