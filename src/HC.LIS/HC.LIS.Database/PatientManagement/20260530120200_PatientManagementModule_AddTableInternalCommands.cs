using FluentMigrator;

namespace HC.LIS.Database.PatientManagement;

[Migration(20260530120200)]
public class PatientManagementModuleAddTableInternalCommands : Migration
{
    public override void Up()
    {
        Create.Table("InternalCommands").InSchema("patient_management")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("EnqueueDate").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
          .WithColumn("Error").AsString(int.MaxValue).Nullable()
        ;
        Create.Index()
          .OnTable("InternalCommands")
          .InSchema("patient_management")
          .OnColumn("EnqueueDate")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InternalCommands").InSchema("patient_management");
    }
}
