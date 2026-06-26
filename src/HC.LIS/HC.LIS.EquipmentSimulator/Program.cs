#pragma warning disable CA1031 // Catching Exception at the program entry point is intentional for fatal-error logging

using HC.LIS.EquipmentSimulator;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    var options = ParseArgs(args);
    if (options is null)
    {
        Log.Error("Usage: HC.LIS.EquipmentSimulator --barcode <value> [--host <host>] [--port <port>] [--delay <ms>]");
        return 1;
    }

    return await new SimulatorRunner(options).RunAsync().ConfigureAwait(false);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Equipment simulator terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync().ConfigureAwait(false);
}

static SimulatorOptions? ParseArgs(string[] args)
{
    string? barcode = null;
    string host = "localhost";
    int port = 8890;
    int delay = 2000;

    for (var i = 0; i < args.Length - 1; i++)
    {
        switch (args[i])
        {
            case "--barcode": barcode = args[i + 1]; break;
            case "--host":   host    = args[i + 1]; break;
            case "--port":   if (int.TryParse(args[i + 1], out var p)) port  = p; break;
            case "--delay":  if (int.TryParse(args[i + 1], out var d)) delay = d; break;
        }
    }

    return barcode is null ? null : new SimulatorOptions { Host = host, Port = port, DelayMs = delay, Barcode = barcode };
}
