using FluentMigrator;

namespace HC.LIS.Database.TestOrders;

[Migration(20260214162000)]
public class TestOrdersModuleAddTableOrderItemDetails : Migration
{
    public override void Up()
    {
        Create.Table("OrderItemDetails").InSchema("test_orders")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OrderId").AsGuid().NotNullable()
          .WithColumn("SpecimenMnemonic").AsString(10).NotNullable()
          .WithColumn("MaterialType").AsString(65).NotNullable()
          .WithColumn("ContainerType").AsString(20).NotNullable()
          .WithColumn("Additive").AsString(20).NotNullable()
          .WithColumn("ProcessingType").AsString(20).NotNullable()
          .WithColumn("StorageCondition").AsString(20).NotNullable()
          .WithColumn("Status").AsString(18).NotNullable()
          .WithColumn("ReasonForRejection").AsString(255).Nullable()
          .WithColumn("RequestedAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("CanceledAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("OnHoldAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("AcceptedAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("RejectedAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("InProgressAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("PartiallyCompletedAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("CompletedAt").AsCustom("TIMESTAMPTZ").Nullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("OrderItemDetails").InSchema("test_orders");
    }
}
