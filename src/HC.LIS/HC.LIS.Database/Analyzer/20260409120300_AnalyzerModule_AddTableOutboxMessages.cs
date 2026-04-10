using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260409120300)]
public class AnalyzerModuleAddTableOutboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("OutboxMessages").InSchema("analyzer")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("OutboxMessages")
          .InSchema("analyzer")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("OutboxMessages").InSchema("analyzer");
    }
}
