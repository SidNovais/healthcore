using System;
using HC.Core.Application;

namespace HC.LIS.TcpMessage.IntegrationTests.Helpers;

public sealed class ExecutionContextMock(Guid userId, string userName) : IExecutionContextAccessor
{
    public Guid UserId { get; } = userId;
    public string UserName { get; } = userName;
    public string CorrelationId { get; } = string.Empty;
    public bool IsAvailable { get; } = true;
}
