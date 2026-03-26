using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260325120100)]
public class LabAnalysisModuleAddTableInboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("InboxMessages").InSchema("lab_analysis")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("InboxMessages")
          .InSchema("lab_analysis")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InboxMessages").InSchema("lab_analysis");
    }
}
