using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260406120600)]
public class LabAnalysisModuleAddTableSignedReportDetails : Migration
{
    public override void Up()
    {
        Create.Table("signed_report_details").InSchema("lab_analysis")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("worklist_item_id").AsGuid().NotNullable()
            .WithColumn("order_id").AsGuid().NotNullable()
            .WithColumn("order_item_id").AsGuid().NotNullable()
            .WithColumn("html_report_path").AsString(1000).Nullable()
            .WithColumn("pdf_report_path").AsString(1000).Nullable()
            .WithColumn("signature").AsCustom("TEXT").NotNullable()
            .WithColumn("signed_by").AsGuid().NotNullable()
            .WithColumn("status").AsString(50).NotNullable()
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable();

        Create.Index("ix_signed_report_details_worklist_item_id")
            .OnTable("signed_report_details").InSchema("lab_analysis")
            .OnColumn("worklist_item_id").Ascending()
            .WithOptions().UniqueIndex();
    }

    public override void Down()
    {
        Delete.Table("signed_report_details").InSchema("lab_analysis");
    }
}
