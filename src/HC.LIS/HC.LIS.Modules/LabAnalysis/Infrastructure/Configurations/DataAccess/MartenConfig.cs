using JasperFx;
using JasperFx.Events;
using Marten;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.DataAccess;

public static class MartenConfig
{
    public static IDocumentStore BuildDocumentStore(string connectionString)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.DatabaseSchemaName = "lab_analysis";
            options.AutoCreateSchemaObjects = AutoCreate.None;
            options.Events.StreamIdentity = StreamIdentity.AsString;
            options.Events.AddEventType<WorklistItemCreatedDomainEvent>();
            options.Events.AddEventType<AnalysisResultRecordedDomainEvent>();
            options.Events.AddEventType<ReportGeneratedDomainEvent>();
            options.Events.AddEventType<WorklistItemCompletedDomainEvent>();
            options.Events.AddEventType<SignedReportCreatedDomainEvent>();
            options.Events.AddEventType<HtmlReportUploadedDomainEvent>();
            options.Events.AddEventType<PdfReportUploadedDomainEvent>();
        });
        return store;
    }
}
