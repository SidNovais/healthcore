using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260418120000)]
public class UserAccessModuleAddSchemaUserAccess : Migration
{
    public override void Up()
    {
        Create.Schema("user_access");
    }

    public override void Down()
    {
        Delete.Schema("user_access");
    }
}
