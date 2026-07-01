using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260628120500)]
public class UserAccessModuleAddTraceContextColumn : Migration
{
    public override void Up()
    {
        Alter.Table("InternalCommands").InSchema("user_access")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("OutboxMessages").InSchema("user_access")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("InboxMessages").InSchema("user_access")
            .AddColumn("TraceContext").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TraceContext").FromTable("InternalCommands").InSchema("user_access");
        Delete.Column("TraceContext").FromTable("OutboxMessages").InSchema("user_access");
        Delete.Column("TraceContext").FromTable("InboxMessages").InSchema("user_access");
    }
}
