using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260126233300)]
public class TestOrdersModuleAddSchemaTestOrders : Migration
{
    public override void Up()
    {
        Create.Schema("test_orders");
    }

    public override void Down()
    {
        Delete.Schema("test_orders");
    }
}
