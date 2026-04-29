using System;
using HC.Core.Application;

namespace HC.LIS.TcpMessage;

internal sealed class SystemExecutionContextAccessor : IExecutionContextAccessor
{
    public Guid UserId => new Guid("00000000-cafe-face-bead-000000000001");
    public string UserName => "tcpmessage-system";
    public string CorrelationId => "system";
    public bool IsAvailable => true;
}
