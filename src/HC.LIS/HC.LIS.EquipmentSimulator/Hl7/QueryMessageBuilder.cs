using System.Text;

namespace HC.LIS.EquipmentSimulator.Hl7;

internal static class QueryMessageBuilder
{
    internal static string Build(string barcode)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"MSH|^~\\&|ANALYZER|SIMULATOR|HC.LIS|SERVER|{timestamp}||QBP^Q11|Q001|P|2.5\r");
        sb.Append(CultureInfo.InvariantCulture, $"QPD|Q11^Sample Info Query^HL70471|Q001|{barcode}\r");
        return sb.ToString();
    }
}
