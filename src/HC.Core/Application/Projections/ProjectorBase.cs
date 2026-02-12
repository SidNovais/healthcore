using HC.Core.Domain;
using System.Threading.Tasks;

namespace HC.Core.Application.Projections;

public abstract class ProjectorBase
{
    protected static Task When(IDomainEvent domainEvent)
    {
        return Task.CompletedTask;
    }
}
