using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260521120000)]
public class SampleCollectionModuleAddSamplesCollectedAtToCollectionRequestDetails : Migration
{
    public override void Up()
    {
        Alter.Table("CollectionRequestDetails").InSchema("sample_collection")
            .AddColumn("SamplesCollectedAt").AsCustom("TIMESTAMPTZ").Nullable();
    }

    public override void Down()
    {
        Delete.Column("SamplesCollectedAt").FromTable("CollectionRequestDetails").InSchema("sample_collection");
    }
}
