using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260418120400)]
public class UserAccessModuleAddTableUsers : Migration
{
    public override void Up()
    {
        Create.Table("users").InSchema("user_access")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("email").AsString(255).NotNullable().Unique()
            .WithColumn("full_name").AsString(255).NotNullable()
            .WithColumn("birthdate").AsDate().NotNullable()
            .WithColumn("gender").AsString(20).NotNullable()
            .WithColumn("role").AsString(50).NotNullable()
            .WithColumn("status").AsString(50).NotNullable()
            .WithColumn("password_hash").AsString(500).Nullable()
            .WithColumn("invitation_token").AsString(100).Nullable()
            .WithColumn("created_at").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("created_by_id").AsGuid().Nullable()
            .WithColumn("activated_at").AsCustom("TIMESTAMPTZ").Nullable();
    }

    public override void Down()
    {
        Delete.Table("users").InSchema("user_access");
    }
}
