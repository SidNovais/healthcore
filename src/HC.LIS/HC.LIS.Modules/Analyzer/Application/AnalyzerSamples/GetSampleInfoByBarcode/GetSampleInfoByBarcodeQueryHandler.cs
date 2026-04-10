using System.Data;
using Dapper;
using HC.Core.Infrastructure.Data;
using HC.LIS.Modules.Analyzer.Application.Configuration.Queries;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;

internal class GetSampleInfoByBarcodeQueryHandler(
    ISqlConnectionFactory sqlConnectionFactory
) : IQueryHandler<GetSampleInfoByBarcodeQuery, SampleInfoDto?>
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory = sqlConnectionFactory;

    public async Task<SampleInfoDto?> Handle(
        GetSampleInfoByBarcodeQuery query,
        CancellationToken cancellationToken
    )
    {
        const string sql = @"
            SELECT
                asd.id                AS ""Id"",
                asd.sample_barcode    AS ""SampleBarcode"",
                asd.patient_name      AS ""PatientName"",
                asd.patient_birthdate AS ""PatientBirthdate"",
                asd.patient_gender    AS ""PatientGender""
            FROM analyzer.analyzer_sample_details AS asd
            WHERE asd.sample_barcode = @SampleBarcode;

            SELECT
                ased.exam_mnemonic    AS ""ExamMnemonic"",
                ased.worklist_item_id AS ""WorklistItemId""
            FROM analyzer.analyzer_sample_exam_details AS ased
            INNER JOIN analyzer.analyzer_sample_details AS asd ON ased.analyzer_sample_id = asd.id
            WHERE asd.sample_barcode = @SampleBarcode
            ORDER BY ased.exam_mnemonic;";

        IDbConnection? connection = _sqlConnectionFactory.GetConnection()
            ?? throw new InvalidOperationException("Must exist connection to get sample info by barcode");

        using SqlMapper.GridReader multi = await connection
            .QueryMultipleAsync(sql, new { query.SampleBarcode })
            .ConfigureAwait(false);

        SampleInfoRow? row = await multi
            .ReadFirstOrDefaultAsync<SampleInfoRow>()
            .ConfigureAwait(false);

        if (row is null)
            return null;

        IEnumerable<SampleExamInfoDto> examRows = await multi
            .ReadAsync<SampleExamInfoDto>()
            .ConfigureAwait(false);

        return new SampleInfoDto
        {
            Id = row.Id,
            SampleBarcode = row.SampleBarcode,
            PatientName = row.PatientName,
            PatientBirthdate = row.PatientBirthdate,
            PatientGender = row.PatientGender,
            Exams = examRows.ToList().AsReadOnly()
        };
    }

    private sealed class SampleInfoRow
    {
        public Guid Id { get; init; }
        public string SampleBarcode { get; init; } = string.Empty;
        public string PatientName { get; init; } = string.Empty;
        public DateTime PatientBirthdate { get; init; }
        public string PatientGender { get; init; } = string.Empty;
    }
}
