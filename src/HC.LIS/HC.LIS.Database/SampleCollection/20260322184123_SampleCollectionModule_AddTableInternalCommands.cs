using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260322184123)]
public class SampleCollectionModuleAddTableInternalCommands : Migration
{
    public override void Up()
    {
        Create.Table("InternalCommands").InSchema("sample_collection")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("EnqueueDate").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
          .WithColumn("Error").AsString(int.MaxValue).Nullable()
        ;
        Create.Index()
          .OnTable("InternalCommands")
          .InSchema("sample_collection")
          .OnColumn("EnqueueDate")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InternalCommands").InSchema("sample_collection");
    }
}
