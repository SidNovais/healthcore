using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;

namespace HC.Core.Infrastructure.RealTime;

/// <summary>
/// Thread-safe, in-memory <see cref="IUiNotificationHub"/>. Each subscriber gets a bounded channel
/// that drops the oldest notification when a slow client fails to drain it, so one stalled browser
/// can never back up the publisher.
/// </summary>
public sealed class UiNotificationHub : IUiNotificationHub
{
    private const int DefaultCapacityPerSubscriber = 100;

    private readonly ConcurrentDictionary<Guid, Subscriber> _subscribers = new();
    private readonly int _capacityPerSubscriber;

    public UiNotificationHub(int capacityPerSubscriber = DefaultCapacityPerSubscriber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(capacityPerSubscriber, 1);

        _capacityPerSubscriber = capacityPerSubscriber;
    }

    public IUiNotificationSubscription Subscribe(IReadOnlyCollection<string> topics)
    {
        ArgumentNullException.ThrowIfNull(topics);

        var channel = Channel.CreateBounded<UiNotification>(new BoundedChannelOptions(_capacityPerSubscriber)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
        });

        var id = Guid.NewGuid();
        var subscriber = new Subscriber(new HashSet<string>(topics, StringComparer.Ordinal), channel);
        _subscribers[id] = subscriber;

        return new Subscription(this, id, channel.Reader);
    }

    public void Publish(UiNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        foreach (Subscriber subscriber in _subscribers.Values)
        {
            if (subscriber.Topics.Contains(notification.Topic))
                subscriber.Channel.Writer.TryWrite(notification);
        }
    }

    private void Remove(Guid id)
    {
        if (_subscribers.TryRemove(id, out Subscriber? subscriber))
            subscriber.Channel.Writer.TryComplete();
    }

    private sealed record Subscriber(HashSet<string> Topics, Channel<UiNotification> Channel);

    private sealed class Subscription(UiNotificationHub hub, Guid id, ChannelReader<UiNotification> reader)
        : IUiNotificationSubscription
    {
        private bool _disposed;

        public ChannelReader<UiNotification> Reader { get; } = reader;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            hub.Remove(id);
        }
    }
}
