using HC.Core.Domain;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class WorklistItemStatus : ValueObject
{
    public string Value { get; }

    public static WorklistItemStatus Pending         => new("Pending");
    public static WorklistItemStatus ResultReceived  => new("ResultReceived");
    public static WorklistItemStatus ReportGenerated => new("ReportGenerated");
    public static WorklistItemStatus Completed       => new("Completed");

    private WorklistItemStatus(string value) => Value = value;

    public static WorklistItemStatus Of(string value) => new(value);

    internal bool IsPending         => Value == "Pending";
    internal bool IsResultReceived  => Value == "ResultReceived";
    internal bool IsReportGenerated => Value == "ReportGenerated";
    internal bool IsCompleted       => Value == "Completed";
}
