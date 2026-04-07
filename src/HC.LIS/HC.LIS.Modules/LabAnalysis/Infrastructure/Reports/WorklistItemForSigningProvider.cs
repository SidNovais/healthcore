using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Reports;

internal class WorklistItemForSigningProvider(
    ISqlConnectionFactory sqlConnectionFactory
) : IWorklistItemForSigningProvider
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<WorklistItemForSigning?> GetAsync(Guid worklistItemId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT order_id, order_item_id, status
            FROM lab_analysis.worklist_item_details
            WHERE id = @WorklistItemId;

            SELECT analyte_code, result_value, result_unit, reference_range, is_out_of_range
            FROM lab_analysis.worklist_item_analyte_results
            WHERE worklist_item_id = @WorklistItemId
            ORDER BY recorded_at;";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get worklist item for signing");

        using SqlMapper.GridReader multi = await connection
            .QueryMultipleAsync(sql, new { WorklistItemId = worklistItemId })
            .ConfigureAwait(false);

        WorklistItemForSigningRow? row = await multi
            .ReadFirstOrDefaultAsync<WorklistItemForSigningRow>()
            .ConfigureAwait(false);

        if (row is null)
            return null;

        IEnumerable<AnalyteResultSnapshotRow> analyteRows = await multi
            .ReadAsync<AnalyteResultSnapshotRow>()
            .ConfigureAwait(false);

        IReadOnlyCollection<AnalyteResultSnapshot> snapshots = analyteRows
            .Select(r => new AnalyteResultSnapshot(r.AnalyteCode, r.ResultValue, r.ResultUnit, r.ReferenceRange, r.IsOutOfRange))
            .ToList()
            .AsReadOnly();

        return WorklistItemForSigning.From(
            worklistItemId,
            row.OrderId,
            row.OrderItemId,
            WorklistItemStatus.Of(row.Status),
            snapshots);
    }

    private sealed class WorklistItemForSigningRow
    {
        public Guid OrderId { get; init; }
        public Guid OrderItemId { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    private sealed class AnalyteResultSnapshotRow
    {
        public string AnalyteCode { get; init; } = string.Empty;
        public string ResultValue { get; init; } = string.Empty;
        public string ResultUnit { get; init; } = string.Empty;
        public string ReferenceRange { get; init; } = string.Empty;
        public bool IsOutOfRange { get; init; }
    }
}
