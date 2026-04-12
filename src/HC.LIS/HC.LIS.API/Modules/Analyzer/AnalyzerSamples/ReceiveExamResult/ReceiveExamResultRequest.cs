namespace HC.LIS.API.Modules.Analyzer.AnalyzerSamples.ReceiveExamResult;

internal sealed record ReceiveExamResultRequest(
    string ExamMnemonic,
    string ResultValue,
    string ResultUnit,
    string ReferenceRange,
    Guid InstrumentId);
