using System;
using HC.Core.Domain;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.WorklistItems;

internal static class WorklistItemFactory
{
    public static WorklistItem CreatePending() =>
        WorklistItem.Create(
            WorklistItemSampleData.WorklistItemId,
            WorklistItemSampleData.SampleId,
            WorklistItemSampleData.SampleBarcode,
            WorklistItemSampleData.ExamCode,
            WorklistItemSampleData.PatientId,
            WorklistItemSampleData.CreatedAt
        );

    public static WorklistItem CreateWithResult()
    {
        WorklistItem item = CreatePending();
        item.RecordResult(
            WorklistItemSampleData.ResultValue,
            WorklistItemSampleData.AnalystId,
            WorklistItemSampleData.RecordedAt
        );
        return item;
    }

    public static WorklistItem CreateWithReport()
    {
        WorklistItem item = CreateWithResult();
        item.GenerateReport(
            WorklistItemSampleData.ReportPath,
            WorklistItemSampleData.GeneratedAt
        );
        return item;
    }
}

internal readonly struct WorklistItemSampleData
{
    public static readonly Guid WorklistItemId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e50");
    public static readonly Guid SampleId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550d0");
    public const string SampleBarcode = "SC-001";
    public const string ExamCode = "019b6c5d-fbf9-7e35-aa12-c38922ec5030";
    public static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550c1");
    public static readonly DateTime CreatedAt = SystemClock.Now;
    public const string ResultValue = "7.4 mmol/L";
    public static readonly Guid AnalystId = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5031");
    public static readonly DateTime RecordedAt = SystemClock.Now;
    public const string ReportPath = "/reports/worklist/SC-001.pdf";
    public static readonly DateTime GeneratedAt = SystemClock.Now;
    public const string CompletionType = "Complete";
    public static readonly DateTime CompletedAt = SystemClock.Now;
}
