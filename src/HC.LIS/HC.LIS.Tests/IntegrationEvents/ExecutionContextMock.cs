using System;
using HC.Core.Application;

namespace HC.LIS.Tests.IntegrationEvents;

public sealed class ExecutionContextMock(Guid userId) : IExecutionContextAccessor
{
    private Guid _userId = userId;

    public Guid UserId => _userId;
    public string UserName => "integration-test";
    public string CorrelationId => "integration-test";
    public bool IsAvailable => true;

    public void SetUserId(Guid userId) => _userId = userId;
}
