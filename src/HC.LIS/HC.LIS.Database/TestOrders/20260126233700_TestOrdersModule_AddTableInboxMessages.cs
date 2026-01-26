using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260126233700)]
public class TestOrdersModuleAddTableInboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("InboxMessages").InSchema("test_orders")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("InboxMessages")
          .InSchema("test_orders")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InboxMessages").InSchema("test_orders");
    }
}
