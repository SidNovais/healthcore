using System;
using HC.Core.Infrastructure.EventBus;

namespace HC.LIS.Modules.LabAnalysis.IntegrationEvents;

public class WorklistItemCreatedIntegrationEvent(
    Guid id,
    DateTime occurredAt,
    Guid worklistItemId,
    Guid patientId,
    string sampleBarcode,
    string examCode,
    string? patientName = null,
    DateTime? patientDateOfBirth = null,
    string? patientGender = null
) : IntegrationEvent(id, occurredAt)
{
    public Guid WorklistItemId { get; } = worklistItemId;
    public Guid PatientId { get; } = patientId;
    public string SampleBarcode { get; } = sampleBarcode;
    public string ExamCode { get; } = examCode;
    public string? PatientName { get; } = patientName;
    public DateTime? PatientDateOfBirth { get; } = patientDateOfBirth;
    public string? PatientGender { get; } = patientGender;
}
