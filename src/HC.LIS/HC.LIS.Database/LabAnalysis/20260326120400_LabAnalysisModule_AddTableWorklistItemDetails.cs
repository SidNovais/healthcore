using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260326120400)]
public class LabAnalysisModuleAddTableWorklistItemDetails : Migration
{
    public override void Up()
    {
        Create.Table("worklist_item_details").InSchema("lab_analysis")
          .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("sample_id").AsGuid().NotNullable()
          .WithColumn("sample_barcode").AsString(255).NotNullable()
          .WithColumn("exam_code").AsString(255).NotNullable()
          .WithColumn("patient_id").AsGuid().NotNullable()
          .WithColumn("status").AsString(50).NotNullable()
          .WithColumn("result_value").AsString(int.MaxValue).Nullable()
          .WithColumn("result_unit").AsString(50).Nullable()
          .WithColumn("reference_range").AsString(100).Nullable()
          .WithColumn("report_path").AsString(500).Nullable()
          .WithColumn("completion_type").AsString(50).Nullable()
          .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("completed_at").AsCustom("TIMESTAMPTZ").Nullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("worklist_item_details").InSchema("lab_analysis");
    }
}
