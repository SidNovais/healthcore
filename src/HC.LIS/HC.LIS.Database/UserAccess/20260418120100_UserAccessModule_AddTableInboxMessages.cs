using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260418120100)]
public class UserAccessModuleAddTableInboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("InboxMessages").InSchema("user_access")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("Type").AsString(255).NotNullable().Indexed()
            .WithColumn("Data").AsString(int.MaxValue).NotNullable()
            .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed();
        Create.Index()
            .OnTable("InboxMessages").InSchema("user_access")
            .OnColumn("OccurredAt").Ascending();
    }

    public override void Down()
    {
        Delete.Table("InboxMessages").InSchema("user_access");
    }
}
