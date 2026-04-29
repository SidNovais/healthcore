using System.Threading.Tasks;
using Dapper;
using HC.Core.IntegrationTests.Probing;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using Npgsql;

namespace HC.LIS.Tests.IntegrationEvents.Probes;

public sealed class GetWorklistItemFromLabAnalysisProbe(
    string sampleBarcode,
    string examCode,
    string connectionString
) : IProbe<WorklistItemDetailsDto>
{
    public string DescribeFailureTo() =>
        $"WorklistItem for barcode '{sampleBarcode}' / exam '{examCode}' not found in LabAnalysis";

    public async Task<WorklistItemDetailsDto?> GetSampleAsync()
    {
        using var connection = new NpgsqlConnection(connectionString);
        return await connection.QueryFirstOrDefaultAsync<WorklistItemDetailsDto>(
            @"SELECT id AS ""Id"", sample_barcode AS ""SampleBarcode"",
                     exam_code AS ""ExamCode"", status AS ""Status"",
                     order_item_id AS ""OrderItemId""
              FROM lab_analysis.worklist_item_details
              WHERE sample_barcode = @Barcode AND exam_code = @ExamCode",
            new { Barcode = sampleBarcode, ExamCode = examCode }
        ).ConfigureAwait(false);
    }

    public bool IsSatisfied(WorklistItemDetailsDto? sample) => sample is not null;
}
