using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260628120100)]
public class LabAnalysisModuleAddTraceContextColumn : Migration
{
    public override void Up()
    {
        Alter.Table("InternalCommands").InSchema("lab_analysis")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("OutboxMessages").InSchema("lab_analysis")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("InboxMessages").InSchema("lab_analysis")
            .AddColumn("TraceContext").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TraceContext").FromTable("InternalCommands").InSchema("lab_analysis");
        Delete.Column("TraceContext").FromTable("OutboxMessages").InSchema("lab_analysis");
        Delete.Column("TraceContext").FromTable("InboxMessages").InSchema("lab_analysis");
    }
}
