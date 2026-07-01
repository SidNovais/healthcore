using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260628120400)]
public class TestOrdersModuleAddTraceContextColumn : Migration
{
    public override void Up()
    {
        Alter.Table("InternalCommands").InSchema("test_orders")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("OutboxMessages").InSchema("test_orders")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("InboxMessages").InSchema("test_orders")
            .AddColumn("TraceContext").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TraceContext").FromTable("InternalCommands").InSchema("test_orders");
        Delete.Column("TraceContext").FromTable("OutboxMessages").InSchema("test_orders");
        Delete.Column("TraceContext").FromTable("InboxMessages").InSchema("test_orders");
    }
}
