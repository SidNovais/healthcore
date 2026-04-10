using System;
using Newtonsoft.Json;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.AssignWorklistItem;

[method: JsonConstructor]
public class AssignWorklistItemByBarcodeAndExamCodeCommand(
    Guid id,
    string sampleBarcode,
    string examCode,
    Guid worklistItemId,
    DateTime assignedAt
) : InternalCommandBase(id)
{
    public string SampleBarcode { get; } = sampleBarcode;
    public string ExamCode { get; } = examCode;
    public Guid WorklistItemId { get; } = worklistItemId;
    public DateTime AssignedAt { get; } = assignedAt;
}
