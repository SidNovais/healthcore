namespace HC.LIS.EquipmentSimulator;

internal sealed class SimulatorOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 8890;
    public int DelayMs { get; init; } = 2000;
    public string Barcode { get; init; } = string.Empty;
}
