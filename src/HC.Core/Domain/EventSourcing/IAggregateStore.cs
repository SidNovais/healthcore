using System.Collections.Generic;
using System.Threading.Tasks;

namespace HC.Core.Domain.EventSourcing;

public interface IAggregateStore
{
    void Start<T>(T aggregate)
      where T : AggregateRoot;
    Task<T?> Load<T>(AggregateId<T> aggregateId)
      where T : AggregateRoot;
    void AppendChanges<T>(T aggregate)
      where T : AggregateRoot;
    IList<IDomainEvent> GetChanges();
    void ClearChanges();
}
