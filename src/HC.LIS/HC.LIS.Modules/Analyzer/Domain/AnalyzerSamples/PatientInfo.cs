using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

public class PatientInfo : ValueObject
{
    public Guid PatientId { get; }
    public string PatientName { get; }
    public DateTime PatientBirthdate { get; }
    public string PatientGender { get; }

    private PatientInfo(
        Guid patientId,
        string patientName,
        DateTime patientBirthdate,
        string patientGender
    )
    {
        PatientId = patientId;
        PatientName = patientName;
        PatientBirthdate = patientBirthdate;
        PatientGender = patientGender;
    }

    public static PatientInfo Of(
        Guid patientId,
        string patientName,
        DateTime patientBirthdate,
        string patientGender
    ) => new(
        patientId,
        patientName,
        patientBirthdate,
        patientGender
    );
}
