using HC.Core.Application.Projections;
using MediatR;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

public class AnalyzerSampleCreatedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<AnalyzerSampleCreatedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        AnalyzerSampleCreatedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
