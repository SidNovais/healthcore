using System;
using System.Collections.Generic;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

public class WorklistItemDetailsDto
{
    public Guid Id { get; set; }
    public Guid SampleId { get; set; }
    public string SampleBarcode { get; set; } = string.Empty;
    public string ExamCode { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string Status { get; set; } = string.Empty;
    public IReadOnlyCollection<AnalyteResultDto> AnalyteResults { get; set; } = Array.Empty<AnalyteResultDto>();
    public string? ReportPath { get; set; }
    public string? CompletionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class AnalyteResultDto
{
    public Guid Id { get; set; }
    public string AnalyteCode { get; set; } = string.Empty;
    public string ResultValue { get; set; } = string.Empty;
    public string ResultUnit { get; set; } = string.Empty;
    public string ReferenceRange { get; set; } = string.Empty;
    public Guid PerformedById { get; set; }
    public DateTime RecordedAt { get; set; }
}
