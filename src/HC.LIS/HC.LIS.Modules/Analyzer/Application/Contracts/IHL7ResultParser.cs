namespace HC.LIS.Modules.Analyzer.Application.Contracts;

public interface IHL7ResultParser
{
    AnalyzerResultDto Parse(string hl7Message);
}
