using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.WorklistItems;

public class ReferenceRangeTests
{
    [Fact]
    public void NumericResultWithinRangeIsNotOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("3.5-5.5 mmol/L");

        range.IsOutOfRange("4.5").Should().BeFalse();
    }

    [Fact]
    public void NumericResultAboveUpperBoundIsOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("3.5-5.5 mmol/L");

        range.IsOutOfRange("7.4").Should().BeTrue();
    }

    [Fact]
    public void NumericResultBelowLowerBoundIsOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("3.5-5.5 mmol/L");

        range.IsOutOfRange("2.0").Should().BeTrue();
    }

    [Fact]
    public void NumericResultAtBoundaryIsNotOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("3.5-5.5 mmol/L");

        range.IsOutOfRange("3.5").Should().BeFalse();
        range.IsOutOfRange("5.5").Should().BeFalse();
    }

    [Fact]
    public void NonParsableResultValueWithNumericRangeIsNotOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("3.5-5.5 mmol/L");

        range.IsOutOfRange("N/A").Should().BeFalse();
    }

    [Fact]
    public void QualitativeResultMatchingReferenceIsNotOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("Negative");

        range.IsOutOfRange("Negative").Should().BeFalse();
    }

    [Fact]
    public void QualitativeResultMatchingReferenceIsCaseInsensitive()
    {
        ReferenceRange range = ReferenceRange.Of("Negative");

        range.IsOutOfRange("NEGATIVE").Should().BeFalse();
    }

    [Fact]
    public void QualitativeResultNotMatchingReferenceIsOutOfRange()
    {
        ReferenceRange range = ReferenceRange.Of("Negative");

        range.IsOutOfRange("Positive").Should().BeTrue();
    }

    [Fact]
    public void RawStringIsPreserved()
    {
        ReferenceRange range = ReferenceRange.Of("3.5-5.5 mmol/L");

        range.Raw.Should().Be("3.5-5.5 mmol/L");
    }
}
