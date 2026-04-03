using System;
using System.Globalization;
using System.Text.RegularExpressions;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public partial class ReferenceRange : ValueObject
{
    [GeneratedRegex(@"^(\d+\.?\d*)-(\d+\.?\d*)", RegexOptions.None)]
    private static partial Regex NumericRangePattern();

    private readonly bool _isNumeric;
    private readonly decimal _lower;
    private readonly decimal _upper;

    public string Raw { get; }

    private ReferenceRange(string raw, bool isNumeric, decimal lower, decimal upper)
    {
        Raw = raw;
        _isNumeric = isNumeric;
        _lower = lower;
        _upper = upper;
    }

    public static ReferenceRange Of(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Match match = NumericRangePattern().Match(value);
        if (match.Success)
        {
            decimal lower = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            decimal upper = decimal.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            return new ReferenceRange(value, true, lower, upper);
        }
        return new ReferenceRange(value, false, 0, 0);
    }

    public bool IsOutOfRange(string resultValue)
    {
        if (_isNumeric)
        {
            if (!decimal.TryParse(resultValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                return false;
            return val < _lower || val > _upper;
        }
        return !string.Equals(resultValue.Trim(), Raw.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
