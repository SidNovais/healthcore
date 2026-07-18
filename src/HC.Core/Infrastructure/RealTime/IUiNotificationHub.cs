using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace HC.Core.Infrastructure.RealTime;

/// <summary>
/// In-process fan-out of real-time notifications to connected browser clients. The hub is
/// transport- and role-agnostic: it filters purely by opaque topic strings, leaving the
/// role → allowed-topics mapping to the caller (the SSE endpoint).
/// </summary>
public interface IUiNotificationHub
{
    /// <summary>
    /// Registers a subscriber interested in <paramref name="topics"/>. Dispose the returned
    /// subscription to unregister and release its buffer.
    /// </summary>
    IUiNotificationSubscription Subscribe(IReadOnlyCollection<string> topics);

    /// <summary>Delivers <paramref name="notification"/> to every subscriber of its topic.</summary>
    void Publish(UiNotification notification);
}

/// <summary>A single subscriber's live feed. Dispose to unsubscribe.</summary>
public interface IUiNotificationSubscription : IDisposable
{
    ChannelReader<UiNotification> Reader { get; }
}
