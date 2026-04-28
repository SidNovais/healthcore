namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;

public class SampleNotFoundException : Exception
{
    public SampleNotFoundException() { }
    public SampleNotFoundException(string message) : base(message) { }
    public SampleNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
