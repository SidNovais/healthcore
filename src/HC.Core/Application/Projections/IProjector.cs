using System.Threading.Tasks;
using HC.Core.Domain;

namespace HC.Core.Application.Projections;

public interface IProjector
{
    Task Project(IDomainEvent domainEvent);
}
