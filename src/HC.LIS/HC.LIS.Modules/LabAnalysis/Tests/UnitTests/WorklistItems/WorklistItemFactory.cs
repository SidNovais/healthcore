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
            WorklistItemSampleData.AnalyteCode,
            WorklistItemSampleData.ResultValue,
            WorklistItemSampleData.ResultUnit,
            WorklistItemSampleData.ReferenceRange,
            WorklistItemSampleData.PerformedById,
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
