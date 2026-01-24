using System;
using HC.Core.Application;

namespace HC.LIS.Modules.TestOrders.IntegrationTests;

public class ExecutionContextMock(Guid Id) : IExecutionContextAccessor
{
    public Guid UserId => Id;
    public string UserName => "John Bobby";
    public string CorrelationId => Guid.CreateVersion7().ToString();
    public bool IsAvailable => true;
}
