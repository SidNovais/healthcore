using System.Diagnostics.Metrics;

namespace HC.Core.Infrastructure.Observability;

public static class HcMeter
{
    public static readonly Meter Instance = new("HC.LIS", "1.0.0");

    public static readonly Counter<long> CommandsExecuted = Instance.CreateCounter<long>(
        "hclis.commands.executed",
        description: "Total number of commands executed");

    public static readonly Histogram<double> CommandDurationMs = Instance.CreateHistogram<double>(
        "hclis.commands.duration_ms",
        unit: "ms",
        description: "Command execution duration in milliseconds");
}
