using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260322185000)]
public class SampleCollectionModuleAddTableCollectionRequestDetails : Migration
{
    public override void Up()
    {
        Create.Table("CollectionRequestDetails").InSchema("sample_collection")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("PatientId").AsGuid().NotNullable()
          .WithColumn("Status").AsString(50).NotNullable()
          .WithColumn("ArrivedAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("WaitingAt").AsCustom("TIMESTAMPTZ").Nullable()
          .WithColumn("CalledAt").AsCustom("TIMESTAMPTZ").Nullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("CollectionRequestDetails").InSchema("sample_collection");
    }
}
