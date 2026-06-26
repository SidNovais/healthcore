using System.Text;

namespace HC.LIS.EquipmentSimulator.Hl7;

internal static class ResultMessageBuilder
{
    internal static string Build(string barcode, IReadOnlyCollection<ExamResult> results)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"MSH|^~\\&|ANALYZER|SIMULATOR|HC.LIS|SERVER|{timestamp}||ORU^R01|R001|P|2.5\r");
        sb.Append(CultureInfo.InvariantCulture, $"SPM|||{barcode}\r");

        var i = 1;
        foreach (var result in results)
        {
            sb.Append(CultureInfo.InvariantCulture, $"OBX|{i}|NM|{result.Mnemonic}^{result.Mnemonic}||{result.Value}|{result.Unit}|{result.ReferenceRange}||F\r");
            i++;
        }

        return sb.ToString();
    }
}
