using System.Security.Claims;
using System.Text;
using HC.Core.Infrastructure.RealTime;
using Microsoft.AspNetCore.Http.Features;

namespace HC.LIS.API.Configuration.RealTime;

internal static class EventStreamEndpoint
{
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(15);

    internal static RouteGroupBuilder MapEventStreamEndpoints(this RouteGroupBuilder group)
    {
        group.WithTags("Events");

        group.MapGet("stream", Handle)
            .WithName("EventStream")
            .WithSummary("Server-Sent Events stream of real-time changes scoped to the caller's role.")
            .RequireAuthorization()
            .ExcludeFromDescription();

        return group;
    }

    private static async Task Handle(HttpContext httpContext, IUiNotificationHub hub, CancellationToken ct)
    {
        string role = httpContext.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        IReadOnlyCollection<string> topics = UiTopics.ForRole(role);

        HttpResponse response = httpContext.Response;

        if (topics.Count == 0)
        {
            response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers["X-Accel-Buffering"] = "no";
        httpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        using IUiNotificationSubscription subscription = hub.Subscribe(topics);

        // Establish the stream and tell the browser to wait 3s before reconnecting.
        await response.WriteAsync("retry: 3000\n\n", ct).ConfigureAwait(false);
        await response.Body.FlushAsync(ct).ConfigureAwait(false);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (await WaitForNextAsync(subscription.Reader, ct).ConfigureAwait(false))
                {
                    while (subscription.Reader.TryRead(out UiNotification? note))
                        await WriteEventAsync(response, note, ct).ConfigureAwait(false);
                }
                else
                {
                    await response.WriteAsync(": heartbeat\n\n", ct).ConfigureAwait(false);
                }

                await response.Body.FlushAsync(ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — nothing to do; the subscription disposes above.
        }
    }

    // Returns true when data is ready to read, false on a heartbeat tick.
    private static async Task<bool> WaitForNextAsync(
        System.Threading.Channels.ChannelReader<UiNotification> reader, CancellationToken ct)
    {
        using var heartbeat = CancellationTokenSource.CreateLinkedTokenSource(ct);
        heartbeat.CancelAfter(HeartbeatInterval);

        try
        {
            return await reader.WaitToReadAsync(heartbeat.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return false;
        }
    }

    private static async Task WriteEventAsync(HttpResponse response, UiNotification note, CancellationToken ct)
    {
        var frame = new StringBuilder()
            .Append("event: ").Append(note.Topic).Append('\n')
            .Append("data: ").Append(note.Data).Append('\n')
            .Append('\n')
            .ToString();

        await response.WriteAsync(frame, ct).ConfigureAwait(false);
    }
}
