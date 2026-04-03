using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.WorklistItems;

internal readonly struct WorklistItemSampleData
{
    public static readonly Guid WorklistItemId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e50");
    public static readonly Guid SampleId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550d0");
    public const string SampleBarcode = "SC-001";
    public const string ExamCode = "019b6c5d-fbf9-7e35-aa12-c38922ec5030";
    public static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550c1");
    public static readonly DateTime CreatedAt = SystemClock.Now;
    public const string AnalyteCode = "WBC";
    public const string ResultValue = "4.5";
    public const string OutOfRangeResultValue = "7.4";
    public const string ResultUnit = "mmol/L";
    public const string ReferenceRange = "3.5-5.5 mmol/L";
    public static readonly Guid PerformedById = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5031");
    public static readonly DateTime RecordedAt = SystemClock.Now;
    public const string ReportPath = "/reports/worklist/SC-001.pdf";
    public static readonly DateTime GeneratedAt = SystemClock.Now;
    public static readonly DateTime CompletedAt = SystemClock.Now;
}
