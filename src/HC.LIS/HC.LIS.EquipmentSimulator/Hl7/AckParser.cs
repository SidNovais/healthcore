namespace HC.LIS.EquipmentSimulator.Hl7;

internal static class AckParser
{
    internal static bool IsAA(string message)
    {
        foreach (var line in message.Split('\r', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("MSA|", StringComparison.Ordinal))
                continue;

            var fields = line.Split('|');
            return fields.Length > 1 && fields[1] == "AA";
        }

        return false;
    }
}
