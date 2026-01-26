using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260126233900)]
public class TestOrdersModuleAddTableInternalCommands : Migration
{
    public override void Up()
    {
        Create.Table("InternalCommands").InSchema("test_orders")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("EnqueueDate").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
          .WithColumn("Error").AsString(int.MaxValue).Nullable()
        ;
        Create.Index()
          .OnTable("InternalCommands")
          .InSchema("test_orders")
          .OnColumn("EnqueueDate")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InternalCommands").InSchema("test_orders");
    }
}
