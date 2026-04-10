using HC.Core.Application.Projections;
using MediatR;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;

public class ExamResultReceivedNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<ExamResultReceivedNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(
        ExamResultReceivedNotification notification,
        CancellationToken cancellationToken
    )
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
