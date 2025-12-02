using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HC.Core.Infrastructure;

public class UnitOfWork(
    DbContext context
) : IUnitOfWork
{
    private readonly DbContext _dbContext = context;
    public async Task<int> CommitAsync(
        Guid? internalCommandId,
        CancellationToken cancellationToken = default
    )
    {
        Task<int> task = _dbContext.SaveChangesAsync(cancellationToken);
        return await task.ConfigureAwait(false);
    }
}
