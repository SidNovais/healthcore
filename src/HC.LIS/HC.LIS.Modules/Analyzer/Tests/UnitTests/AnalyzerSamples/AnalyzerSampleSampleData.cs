using System;
using HC.Core.Domain;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.UnitTests.AnalyzerSamples;

public readonly struct AnalyzerSampleSampleData
{
    public static readonly Guid AnalyzerSampleId = Guid.Parse("019f4a10-1b2c-7d3e-8f4a-5b6c7d8e9f00");
    public static readonly Guid SampleId = Guid.Parse("019f4a10-2c3d-7e4f-9a5b-6c7d8e9f0a11");
    public static readonly Guid PatientId = Guid.Parse("019f4a10-3d4e-7f50-ab6c-7d8e9f0a1b22");
    public static readonly string SampleBarcode = "SMP-20260409-001";
    public static readonly string PatientName = "John Doe";
    public static readonly DateTime PatientBirthdate = new(1990, 1, 15);
    public static readonly string PatientGender = "Male";
    public static readonly PatientInfo PatientInfo = PatientInfo.Of(PatientId, PatientName, PatientBirthdate, PatientGender);
    public static readonly Guid ExamId = Guid.Parse("019f4a10-4e5f-7061-bc7d-8e9f0a1b2c33");
    public static readonly string ExamMnemonic = "CBC";
    public static readonly Guid ExamId2 = Guid.Parse("019f4a10-5f60-7172-cd8e-9f0a1b2c3d44");
    public static readonly string ExamMnemonic2 = "BMP";
    public static readonly Guid WorklistItemId = Guid.Parse("019f4a10-6071-7283-de9f-0a1b2c3d4e55");
    public static readonly Guid WorklistItemId2 = Guid.Parse("019f4a10-7182-7394-ef0a-1b2c3d4e5f66");
    public static readonly Guid InstrumentId = Guid.Parse("019f4a10-8293-74a5-f01b-2c3d4e5f6077");
    public static readonly string ResultValue = "5.0";
    public static readonly string ResultUnit = "mg/dL";
    public static readonly string ReferenceRange = "3.5-7.0";
    public static readonly DateTime CreatedAt = SystemClock.Now;
}
