using HC.Core.Application.Projections;
using MediatR;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;

public class SampleInfoDispatchedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<SampleInfoDispatchedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        SampleInfoDispatchedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
