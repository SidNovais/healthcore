namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

public record WorklistItemSummaryDto(
    Guid Id,
    string SampleBarcode,
    string ExamCode,
    Guid PatientId,
    string Status,
    DateTime CreatedAt);
