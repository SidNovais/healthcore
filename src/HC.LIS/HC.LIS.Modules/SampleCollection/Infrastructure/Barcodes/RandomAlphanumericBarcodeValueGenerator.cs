namespace HC.LIS.Modules.SampleCollection.Infrastructure.Barcodes;

internal class RandomAlphanumericBarcodeValueGenerator : Application.IBarcodeValueGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int Length = 8;

    public string Generate()
    {
        var chars = new char[Length];
        for (var i = 0; i < Length; i++)
        {
#pragma warning disable CA5394
            chars[i] = Alphabet[Random.Shared.Next(Alphabet.Length)];
#pragma warning restore CA5394
        }
        return new string(chars);
    }
}
