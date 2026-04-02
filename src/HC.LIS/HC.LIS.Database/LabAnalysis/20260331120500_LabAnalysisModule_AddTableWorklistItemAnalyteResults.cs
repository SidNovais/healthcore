using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260331120500)]
public class LabAnalysisModuleAddTableWorklistItemAnalyteResults : Migration
{
    public override void Up()
    {
        Create.Table("worklist_item_analyte_results").InSchema("lab_analysis")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("worklist_item_id").AsGuid().NotNullable()
            .WithColumn("analyte_code").AsString(100).NotNullable()
            .WithColumn("result_value").AsString(int.MaxValue).NotNullable()
            .WithColumn("result_unit").AsString(50).NotNullable()
            .WithColumn("reference_range").AsString(100).NotNullable()
            .WithColumn("performed_by_id").AsGuid().NotNullable()
            .WithColumn("recorded_at").AsCustom("TIMESTAMPTZ").NotNullable()
        ;

        Create.Index("ix_worklist_item_analyte_results_worklist_item_id")
            .OnTable("worklist_item_analyte_results").InSchema("lab_analysis")
            .OnColumn("worklist_item_id").Ascending();

        Create.UniqueConstraint("uq_worklist_item_analyte_results_worklist_item_id_analyte_code")
            .OnTable("worklist_item_analyte_results").WithSchema("lab_analysis")
            .Columns("worklist_item_id", "analyte_code");
    }

    public override void Down()
    {
        Delete.Table("worklist_item_analyte_results").InSchema("lab_analysis");
    }
}
