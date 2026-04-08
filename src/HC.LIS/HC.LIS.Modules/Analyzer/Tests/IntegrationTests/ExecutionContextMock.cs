using System;
using HC.Core.Application;

namespace HC.LIS.Modules.Analyzer.IntegrationTests;

public class ExecutionContextMock(Guid userId, string userName) : IExecutionContextAccessor
{
    public Guid UserId { get; } = userId;
    public string UserName { get; } = userName;
    public string CorrelationId { get; } = string.Empty;
    public bool IsAvailable { get; } = true;
}
