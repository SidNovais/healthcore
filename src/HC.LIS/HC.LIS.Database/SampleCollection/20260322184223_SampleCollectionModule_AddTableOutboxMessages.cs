using FluentMigrator;

namespace HC.LIS.Database.SampleCollection;

[Migration(20260322184223)]
public class SampleCollectionModuleAddTableOutboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("OutboxMessages").InSchema("sample_collection")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("OutboxMessages")
          .InSchema("sample_collection")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("OutboxMessages").InSchema("sample_collection");
    }
}
