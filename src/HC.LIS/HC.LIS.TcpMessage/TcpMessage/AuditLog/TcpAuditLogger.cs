using HC.LIS.TcpMessage.Tcp;
using Microsoft.Extensions.Logging;

namespace HC.LIS.TcpMessage.AuditLog;

internal sealed partial class TcpAuditLogger(ILogger<TcpAuditLogger> logger)
{
    internal void LogInbound(string connectionIp, int messageSizeBytes, ConnectionState state) =>
        Log.Inbound(logger, connectionIp, messageSizeBytes, state);

    internal void LogOutbound(string connectionIp, int messageSizeBytes, ConnectionState state) =>
        Log.Outbound(logger, connectionIp, messageSizeBytes, state);

    private static partial class Log
    {
        [LoggerMessage(Level = LogLevel.Information,
            Message = "[Inbound] IP={ConnectionIp} Size={MessageSizeBytes}B State={State}")]
        internal static partial void Inbound(
            ILogger logger, string connectionIp, int messageSizeBytes, ConnectionState state);

        [LoggerMessage(Level = LogLevel.Information,
            Message = "[Outbound] IP={ConnectionIp} Size={MessageSizeBytes}B State={State}")]
        internal static partial void Outbound(
            ILogger logger, string connectionIp, int messageSizeBytes, ConnectionState state);
    }
}
