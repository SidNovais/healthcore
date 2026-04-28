namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.HandleBarcodeQuery;

public interface IHL7QueryParser
{
    string ParseBarcode(byte[] rawQueryPayload);
}
