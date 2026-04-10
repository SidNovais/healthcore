namespace HC.LIS.Modules.Analyzer.Application.Contracts;

public record AnalyzerResultDto(
    string SampleBarcode,
    string ExamMnemonic,
    string ResultValue,
    string ResultUnit,
    string ReferenceRange,
    Guid InstrumentId,
    DateTime RecordedAt
);
