using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260322185100)]
public class SampleCollectionModuleAddTableSampleDetails : Migration
{
    public override void Up()
    {
        Create.Table("SampleDetails").InSchema("sample_collection")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("CollectionRequestId").AsGuid().NotNullable()
          .WithColumn("TubeType").AsString(255).NotNullable()
          .WithColumn("Barcode").AsString(255).Nullable()
          .WithColumn("Status").AsString(50).NotNullable()
          .WithColumn("CollectedAt").AsCustom("TIMESTAMPTZ").Nullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("SampleDetails").InSchema("sample_collection");
    }
}
