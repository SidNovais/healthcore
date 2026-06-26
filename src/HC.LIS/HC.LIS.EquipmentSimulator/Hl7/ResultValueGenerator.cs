#pragma warning disable CA5394 // Random is sufficient for simulating non-security test lab data

namespace HC.LIS.EquipmentSimulator.Hl7;

internal sealed record ExamResult(string Mnemonic, string Value, string Unit, string ReferenceRange);

internal static class ResultValueGenerator
{
    private static readonly Random s_random = new();

    private static readonly Dictionary<string, (int Min, int Max, string Unit, string RefRange)> s_knownAnalytes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["GLUCOSE"]     = (70,  110, "mg/dL", "70-100"),
            ["CHOLESTEROL"] = (140, 220, "mg/dL", "<200"),
            ["HGB"]         = (11,  17,  "g/dL",  "12-17"),
            ["WBC"]         = (4,   11,  "K/uL",  "4.5-11"),
            ["PLT"]         = (150, 400, "K/uL",  "150-400"),
        };

    internal static ExamResult Generate(string mnemonic)
    {
        if (s_knownAnalytes.TryGetValue(mnemonic, out var spec))
            return new ExamResult(mnemonic, s_random.Next(spec.Min, spec.Max + 1).ToString(CultureInfo.InvariantCulture), spec.Unit, spec.RefRange);

        return new ExamResult(mnemonic, s_random.Next(1, 101).ToString(CultureInfo.InvariantCulture), "U/L", "0-100");
    }
}
