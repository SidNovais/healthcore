using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260418120200)]
public class UserAccessModuleAddTableInternalCommands : Migration
{
    public override void Up()
    {
        Create.Table("InternalCommands").InSchema("user_access")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("EnqueueDate").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("Type").AsString(255).NotNullable().Indexed()
            .WithColumn("Data").AsString(int.MaxValue).NotNullable()
            .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
            .WithColumn("Error").AsString(int.MaxValue).Nullable();
        Create.Index()
            .OnTable("InternalCommands").InSchema("user_access")
            .OnColumn("EnqueueDate").Ascending();
    }

    public override void Down()
    {
        Delete.Table("InternalCommands").InSchema("user_access");
    }
}
