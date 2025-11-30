using System;

namespace HC.Core.Application;

public interface IExecutionContextAccessor
{
    Guid UserId { get; }
    string UserName { get; }
    string CorrelationId { get; }
    bool IsAvailable { get; }
}
