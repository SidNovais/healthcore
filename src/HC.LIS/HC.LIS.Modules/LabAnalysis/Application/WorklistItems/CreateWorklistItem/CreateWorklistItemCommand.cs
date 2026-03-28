using System;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.CreateWorklistItem;

public class CreateWorklistItemCommand(
    Guid worklistItemId,
    Guid sampleId,
    string sampleBarcode,
    string examCode,
    Guid patientId,
    DateTime createdAt
) : CommandBase<Guid>
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid SampleId { get; } = sampleId;
    public string SampleBarcode { get; } = sampleBarcode;
    public string ExamCode { get; } = examCode;
    public Guid PatientId { get; } = patientId;
    public DateTime CreatedAt { get; } = createdAt;
}
