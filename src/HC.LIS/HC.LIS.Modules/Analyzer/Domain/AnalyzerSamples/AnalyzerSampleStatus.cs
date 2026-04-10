using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

public class AnalyzerSampleStatus : ValueObject
{
    public string Value { get; }
    public static AnalyzerSampleStatus AwaitingQuery => new("AwaitingQuery");
    public static AnalyzerSampleStatus InfoDispatched => new("InfoDispatched");
    public static AnalyzerSampleStatus ResultReceived => new("ResultReceived");
    private AnalyzerSampleStatus(string value)
    {
        Value = value;
    }
    public static AnalyzerSampleStatus Of(string value) => new(value);
    internal bool IsAwaitingQuery => Value == "AwaitingQuery";
    internal bool IsInfoDispatched => Value == "InfoDispatched";
    internal bool IsResultReceived => Value == "ResultReceived";
}
