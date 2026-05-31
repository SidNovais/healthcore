using FluentMigrator;

namespace HC.LIS.Database.PatientManagement;

[Migration(20260530120300)]
public class PatientManagementModuleAddTableOutboxMessages : Migration
{
    public override void Up()
    {
        Create.Table("OutboxMessages").InSchema("patient_management")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("OccurredAt").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
        ;
        Create.Index()
          .OnTable("OutboxMessages")
          .InSchema("patient_management")
          .OnColumn("OccurredAt")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("OutboxMessages").InSchema("patient_management");
    }
}
