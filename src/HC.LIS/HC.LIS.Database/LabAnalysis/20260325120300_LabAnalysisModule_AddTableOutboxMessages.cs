using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260325120300)]
public class LabAnalysisModuleAddTableOutboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("OutboxMessages").InSchema("lab_analysis")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("OutboxMessages")
          .InSchema("lab_analysis")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("OutboxMessages").InSchema("lab_analysis");
    }
}
