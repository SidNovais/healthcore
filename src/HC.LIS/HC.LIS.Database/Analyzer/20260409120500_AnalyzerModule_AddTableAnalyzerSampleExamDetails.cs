using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260409120500)]
public class AnalyzerModuleAddTableAnalyzerSampleExamDetails : Migration
{
    public override void Up()
    {
        Create.Table("analyzer_sample_exam_details").InSchema("analyzer")
          .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("analyzer_sample_id").AsGuid().NotNullable()
          .WithColumn("exam_mnemonic").AsString(255).NotNullable()
          .WithColumn("worklist_item_id").AsGuid().Nullable()
          .WithColumn("result_value").AsString(int.MaxValue).Nullable()
          .WithColumn("result_unit").AsString(50).Nullable()
          .WithColumn("reference_range").AsString(255).Nullable()
          .WithColumn("instrument_id").AsGuid().Nullable()
          .WithColumn("recorded_at").AsCustom("TIMESTAMPTZ").Nullable()
        ;

        Create.Index("ix_analyzer_sample_exam_details_analyzer_sample_id")
          .OnTable("analyzer_sample_exam_details").InSchema("analyzer")
          .OnColumn("analyzer_sample_id").Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("analyzer_sample_exam_details").InSchema("analyzer");
    }
}
