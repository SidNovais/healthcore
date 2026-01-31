using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260131202100)]
public class TestOrdersModuleAddTableOrderDetails : Migration
{
    public override void Up()
    {
        Create.Table("OrderDetails").InSchema("test_orders")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("PatientId").AsGuid().NotNullable()
          .WithColumn("Priority").AsString(7).NotNullable()
          .WithColumn("RequestedBy").AsGuid().NotNullable()
          .WithColumn("RequestedAt").AsCustom("TIMESTAMPTZ").NotNullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("OrderDetails").InSchema("test_orders");
    }
}
