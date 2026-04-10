using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetAnalyzerSampleDetails;

internal class GetAnalyzerSampleDetailsQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetAnalyzerSampleDetailsQuery, AnalyzerSampleDetailsDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<AnalyzerSampleDetailsDto?> Handle(
        GetAnalyzerSampleDetailsQuery query,
        CancellationToken cancellationToken
    )
    {
        string sql = @$"
            SELECT
                asd.id                AS ""{nameof(AnalyzerSampleDetailsDto.Id)}"",
                asd.sample_id         AS ""{nameof(AnalyzerSampleDetailsDto.SampleId)}"",
                asd.patient_id        AS ""{nameof(AnalyzerSampleDetailsDto.PatientId)}"",
                asd.sample_barcode    AS ""{nameof(AnalyzerSampleDetailsDto.SampleBarcode)}"",
                asd.patient_name      AS ""{nameof(AnalyzerSampleDetailsDto.PatientName)}"",
                asd.patient_birthdate AS ""{nameof(AnalyzerSampleDetailsDto.PatientBirthdate)}"",
                asd.patient_gender    AS ""{nameof(AnalyzerSampleDetailsDto.PatientGender)}"",
                asd.status            AS ""{nameof(AnalyzerSampleDetailsDto.Status)}"",
                asd.dispatched_at     AS ""{nameof(AnalyzerSampleDetailsDto.DispatchedAt)}"",
                asd.created_at        AS ""{nameof(AnalyzerSampleDetailsDto.CreatedAt)}""
            FROM analyzer.analyzer_sample_details AS asd
            WHERE asd.id = @AnalyzerSampleId";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get analyzer sample details");

        return await connection
            .QueryFirstOrDefaultAsync<AnalyzerSampleDetailsDto>(sql, new { query.AnalyzerSampleId })
            .ConfigureAwait(false);
    }
}
