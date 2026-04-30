using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260430000000)]
public class AnalyzerModuleAddUniqueConstraintAnalyzerSampleExamDetails : Migration
{
    public override void Up()
    {
        Create.UniqueConstraint("uq_analyzer_sample_exam_details_sample_exam")
            .OnTable("analyzer_sample_exam_details").WithSchema("analyzer")
            .Columns("analyzer_sample_id", "exam_mnemonic");
    }

    public override void Down()
    {
        Delete.UniqueConstraint("uq_analyzer_sample_exam_details_sample_exam")
            .FromTable("analyzer_sample_exam_details").InSchema("analyzer");
    }
}
