using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace HC.Core.Infrastructure.Observability;

public class TracingCommandHandlerWithResultDecorator<TRequest, TResponse>(
    IRequestHandler<TRequest, TResponse> decorated,
    ActivitySource activitySource
) : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _decorated = decorated;
    private readonly ActivitySource _activitySource = activitySource;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(
            typeof(TRequest).Name,
            ActivityKind.Internal);

        activity?.SetTag("request.type", typeof(TRequest).FullName);

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await _decorated.Handle(request, cancellationToken).ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
            HcMeter.CommandsExecuted.Add(1,
                new KeyValuePair<string, object?>("command", typeof(TRequest).Name),
                new KeyValuePair<string, object?>("outcome", "success"));
            return result;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.AddEvent(new ActivityEvent("exception",
                tags: new ActivityTagsCollection
                {
                    ["exception.type"] = exception.GetType().FullName,
                    ["exception.message"] = exception.Message
                }));
            HcMeter.CommandsExecuted.Add(1,
                new KeyValuePair<string, object?>("command", typeof(TRequest).Name),
                new KeyValuePair<string, object?>("outcome", "failure"));
            throw;
        }
        finally
        {
            sw.Stop();
            HcMeter.CommandDurationMs.Record(sw.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("command", typeof(TRequest).Name));
        }
    }
}
