using MediatR;
using HC.Core.Application.Projections;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.AddExamToCollection;

public class SampleCreatedForExamNotificationProjection(
    IList<IProjector> projectors
) : INotificationHandler<SampleCreatedForExamNotification>
{
    private readonly IList<IProjector> _projectors = projectors;

    public async Task Handle(SampleCreatedForExamNotification notification, CancellationToken cancellationToken)
    {
        foreach (var projector in _projectors)
            await projector.Project(notification.DomainEvent).ConfigureAwait(false);
    }
}
