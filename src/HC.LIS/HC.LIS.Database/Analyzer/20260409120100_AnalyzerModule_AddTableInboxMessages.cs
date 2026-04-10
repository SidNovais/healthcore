using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260409120100)]
public class AnalyzerModuleAddTableInboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("InboxMessages").InSchema("analyzer")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("InboxMessages")
          .InSchema("analyzer")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InboxMessages").InSchema("analyzer");
    }
}
