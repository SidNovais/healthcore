namespace HC.LIS.EquipmentSimulator.Hl7;

internal static class ResponseParser
{
    internal static string[] ParseExamMnemonics(string message)
    {
        foreach (var line in message.Split('\r', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("OBR|", StringComparison.Ordinal))
                continue;

            var fields = line.Split('|');
            if (fields.Length <= 4)
                return [];

            return fields[4].Split('~', StringSplitOptions.RemoveEmptyEntries);
        }

        return [];
    }
}
