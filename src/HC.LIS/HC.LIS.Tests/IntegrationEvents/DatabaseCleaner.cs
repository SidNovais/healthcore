using System.Data;
using System.Threading.Tasks;
using Dapper;

namespace HC.LIS.Tests.IntegrationEvents;

internal static class DatabaseCleaner
{
    internal static async Task ClearAllAsync(IDbConnection connection)
    {
        await ClearTestOrdersAsync(connection);
        await ClearSampleCollectionAsync(connection);
        await ClearAnalyzerAsync(connection);
        await ClearLabAnalysisAsync(connection);
    }

    private static async Task ClearTestOrdersAsync(IDbConnection connection)
        => await connection.ExecuteAsync(@"
            DELETE FROM ""test_orders"".""InboxMessages"";
            DELETE FROM ""test_orders"".""InternalCommands"";
            DELETE FROM ""test_orders"".""OutboxMessages"";
            DELETE FROM ""test_orders"".""OrderItemDetails"";
            DELETE FROM ""test_orders"".""OrderDetails"";
            DELETE FROM ""test_orders"".""mt_events"";
            DELETE FROM ""test_orders"".""mt_streams"";");

    private static async Task ClearSampleCollectionAsync(IDbConnection connection)
        => await connection.ExecuteAsync(@"
            DELETE FROM ""sample_collection"".""InboxMessages"";
            DELETE FROM ""sample_collection"".""InternalCommands"";
            DELETE FROM ""sample_collection"".""OutboxMessages"";
            DELETE FROM ""sample_collection"".""SampleDetails"";
            DELETE FROM ""sample_collection"".""CollectionRequestDetails"";
            DELETE FROM ""sample_collection"".""mt_events"";
            DELETE FROM ""sample_collection"".""mt_streams"";");

    private static async Task ClearAnalyzerAsync(IDbConnection connection)
        => await connection.ExecuteAsync(@"
            DELETE FROM ""analyzer"".""InboxMessages"";
            DELETE FROM ""analyzer"".""InternalCommands"";
            DELETE FROM ""analyzer"".""OutboxMessages"";
            DELETE FROM ""analyzer"".""analyzer_sample_exam_details"";
            DELETE FROM ""analyzer"".""analyzer_sample_details"";
            DELETE FROM ""analyzer"".""mt_doc_deadletterevent"";
            DELETE FROM ""analyzer"".""mt_event_progression"";
            DELETE FROM ""analyzer"".""mt_events"";
            DELETE FROM ""analyzer"".""mt_streams"";");

    private static async Task ClearLabAnalysisAsync(IDbConnection connection)
        => await connection.ExecuteAsync(@"
            DELETE FROM ""lab_analysis"".""InboxMessages"";
            DELETE FROM ""lab_analysis"".""InternalCommands"";
            DELETE FROM ""lab_analysis"".""OutboxMessages"";
            DELETE FROM ""lab_analysis"".""worklist_item_analyte_results"";
            DELETE FROM ""lab_analysis"".""signed_report_details"";
            DELETE FROM ""lab_analysis"".""worklist_item_details"";
            DELETE FROM ""lab_analysis"".""mt_events"";
            DELETE FROM ""lab_analysis"".""mt_streams"";");
}
