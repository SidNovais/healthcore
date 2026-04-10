using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260409120200)]
public class AnalyzerModuleAddTableInternalCommands : Migration
{
    public override void Up()
    {
        Create.Table("InternalCommands").InSchema("analyzer")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("EnqueueDate").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
          .WithColumn("Error").AsString(int.MaxValue).Nullable()
        ;
        Create.Index()
          .OnTable("InternalCommands")
          .InSchema("analyzer")
          .OnColumn("EnqueueDate")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InternalCommands").InSchema("analyzer");
    }
}
