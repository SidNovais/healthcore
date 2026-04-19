using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260418120500)]
public class UserAccessModuleAddTableAuditLog : Migration
{
    public override void Up()
    {
        Create.Table("audit_log").InSchema("user_access")
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("occurred_at").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("user_id").AsGuid().Nullable()
            .WithColumn("actor_id").AsGuid().Nullable()
            .WithColumn("event_type").AsString(50).NotNullable()
            .WithColumn("details").AsString(int.MaxValue).Nullable();
        Create.Index()
            .OnTable("audit_log").InSchema("user_access")
            .OnColumn("occurred_at").Descending();
    }

    public override void Down()
    {
        Delete.Table("audit_log").InSchema("user_access");
    }
}
