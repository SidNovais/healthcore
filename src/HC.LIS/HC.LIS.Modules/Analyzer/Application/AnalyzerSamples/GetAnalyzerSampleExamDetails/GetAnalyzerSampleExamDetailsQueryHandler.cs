using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleExamDetails;

internal class GetAnalyzerSampleExamDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetAnalyzerSampleExamDetailsQuery, IReadOnlyCollection<AnalyzerSampleExamDetailsDto>>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<IReadOnlyCollection<AnalyzerSampleExamDetailsDto>> Handle(
        GetAnalyzerSampleExamDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"
            SELECT
                ased.id                  AS ""{nameof(AnalyzerSampleExamDetailsDto.Id)}"",
                ased.analyzer_sample_id  AS ""{nameof(AnalyzerSampleExamDetailsDto.AnalyzerSampleId)}"",
                ased.exam_mnemonic       AS ""{nameof(AnalyzerSampleExamDetailsDto.ExamMnemonic)}"",
                ased.worklist_item_id    AS ""{nameof(AnalyzerSampleExamDetailsDto.WorklistItemId)}"",
                ased.result_value        AS ""{nameof(AnalyzerSampleExamDetailsDto.ResultValue)}"",
                ased.result_unit         AS ""{nameof(AnalyzerSampleExamDetailsDto.ResultUnit)}"",
                ased.reference_range     AS ""{nameof(AnalyzerSampleExamDetailsDto.ReferenceRange)}"",
                ased.instrument_id       AS ""{nameof(AnalyzerSampleExamDetailsDto.InstrumentId)}"",
                ased.recorded_at         AS ""{nameof(AnalyzerSampleExamDetailsDto.RecordedAt)}""
            FROM analyzer.analyzer_sample_exam_details AS ased
            WHERE ased.analyzer_sample_id = @AnalyzerSampleId
            ORDER BY ased.exam_mnemonic";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get analyzer sample exam details");

        IEnumerable<AnalyzerSampleExamDetailsDto> results = await connection
            .QueryAsync<AnalyzerSampleExamDetailsDto>(sql, new { query.AnalyzerSampleId })
            .ConfigureAwait(false);

        return results.ToList().AsReadOnly();
    }
}
