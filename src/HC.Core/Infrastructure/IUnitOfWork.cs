using System;
using System.Threading;
using System.Threading.Tasks;

namespace HC.Core.Infrastructure;

public interface IUnitOfWork
{
    Task<int> CommitAsync(
      Guid? internalCommandId,
      CancellationToken cancellationToken = default
    );
}
