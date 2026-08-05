namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemList;

public record WorklistItemSummaryDto(
    Guid Id,
    string SampleBarcode,
    string ExamCode,
    Guid PatientId,
    string? PatientName,
    DateTime? PatientDateOfBirth,
    string? PatientGender,
    string? RequestedByName,
    string Status,
    DateTime CreatedAt);
