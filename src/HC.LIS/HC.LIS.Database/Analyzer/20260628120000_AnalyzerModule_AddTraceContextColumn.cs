using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260628120000)]
public class AnalyzerModuleAddTraceContextColumn : Migration
{
    public override void Up()
    {
        Alter.Table("InternalCommands").InSchema("analyzer")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("OutboxMessages").InSchema("analyzer")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("InboxMessages").InSchema("analyzer")
            .AddColumn("TraceContext").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TraceContext").FromTable("InternalCommands").InSchema("analyzer");
        Delete.Column("TraceContext").FromTable("OutboxMessages").InSchema("analyzer");
        Delete.Column("TraceContext").FromTable("InboxMessages").InSchema("analyzer");
    }
}
