using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using HC.Core.Domain;
using System.Linq;

namespace HC.Core.Infrastructure.DomainEventsDispatching;

public class DomainEventsAccessor(DbContext dbContext) : IDomainEventsAccessor
{
    private readonly DbContext _dbContext = dbContext;

    public IReadOnlyCollection<IDomainEvent> GetAllDomainEvents()
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Count != 0).ToList();
        return domainEntities
            .SelectMany(x => x.Entity.Events)
            .ToList();
    }

    public void ClearAllDomainEvents()
    {
        var domainEntities = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.Events != null && x.Entity.Events.Count != 0).ToList();
        domainEntities.ForEach(entity => entity.Entity.ClearEvents());
    }
}
