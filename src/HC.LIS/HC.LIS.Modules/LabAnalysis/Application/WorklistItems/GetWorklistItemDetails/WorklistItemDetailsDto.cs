using System;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

public class WorklistItemDetailsDto
{
    public Guid Id { get; set; }
    public Guid SampleId { get; set; }
    public string SampleBarcode { get; set; } = string.Empty;
    public string ExamCode { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResultValue { get; set; }
    public string? ResultUnit { get; set; }
    public string? ReferenceRange { get; set; }
    public string? ReportPath { get; set; }
    public string? CompletionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
