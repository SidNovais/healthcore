using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260409120400)]
public class AnalyzerModuleAddTableAnalyzerSampleDetails : Migration
{
    public override void Up()
    {
        Create.Table("analyzer_sample_details").InSchema("analyzer")
          .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("sample_id").AsGuid().NotNullable()
          .WithColumn("patient_id").AsGuid().NotNullable()
          .WithColumn("sample_barcode").AsString(255).NotNullable()
          .WithColumn("patient_name").AsString(255).NotNullable()
          .WithColumn("patient_birthdate").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("patient_gender").AsString(10).NotNullable()
          .WithColumn("is_urgent").AsBoolean().NotNullable().WithDefaultValue(false)
          .WithColumn("status").AsString(50).NotNullable()
          .WithColumn("dispatched_at").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("analyzer_sample_details").InSchema("analyzer");
    }
}
