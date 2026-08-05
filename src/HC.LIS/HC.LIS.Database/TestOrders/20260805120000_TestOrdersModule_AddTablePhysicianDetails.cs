using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260805120000)]
public class TestOrdersModuleAddTablePhysicianDetails : Migration
{
    // No foreign key from "OrderDetails"."RequestedBy": historical rows hold UserAccess user ids,
    // which have no matching physician and would violate the constraint.
    public override void Up()
    {
        Create.Table("PhysicianDetails").InSchema("test_orders")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("FullName").AsString(255).NotNullable()
            .WithColumn("LicenceNumber").AsString(100).Nullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("RegisteredAt").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("UpdatedAt").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("DeactivatedAt").AsCustom("TIMESTAMPTZ").Nullable()
        ;

        Create.Index()
          .OnTable("PhysicianDetails")
          .InSchema("test_orders")
          .OnColumn("FullName")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("PhysicianDetails").InSchema("test_orders");
    }
}
