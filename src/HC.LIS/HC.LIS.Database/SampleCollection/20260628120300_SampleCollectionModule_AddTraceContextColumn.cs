using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260628120300)]
public class SampleCollectionModuleAddTraceContextColumn : Migration
{
    public override void Up()
    {
        Alter.Table("InternalCommands").InSchema("sample_collection")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("OutboxMessages").InSchema("sample_collection")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("InboxMessages").InSchema("sample_collection")
            .AddColumn("TraceContext").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TraceContext").FromTable("InternalCommands").InSchema("sample_collection");
        Delete.Column("TraceContext").FromTable("OutboxMessages").InSchema("sample_collection");
        Delete.Column("TraceContext").FromTable("InboxMessages").InSchema("sample_collection");
    }
}
