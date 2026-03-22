using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260322183923)]
public class SampleCollectionModuleAddSchema : Migration
{
    public override void Up()
    {
        Create.Schema("sample_collection");
    }

    public override void Down()
    {
        Delete.Schema("sample_collection");
    }
}
