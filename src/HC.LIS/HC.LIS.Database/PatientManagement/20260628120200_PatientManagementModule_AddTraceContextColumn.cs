using FluentMigrator;

namespace HC.LIS.Database.PatientManagement;

[Migration(20260628120200)]
public class PatientManagementModuleAddTraceContextColumn : Migration
{
    public override void Up()
    {
        Alter.Table("InternalCommands").InSchema("patient_management")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("OutboxMessages").InSchema("patient_management")
            .AddColumn("TraceContext").AsString(100).Nullable();
        Alter.Table("InboxMessages").InSchema("patient_management")
            .AddColumn("TraceContext").AsString(100).Nullable();
    }

    public override void Down()
    {
        Delete.Column("TraceContext").FromTable("InternalCommands").InSchema("patient_management");
        Delete.Column("TraceContext").FromTable("OutboxMessages").InSchema("patient_management");
        Delete.Column("TraceContext").FromTable("InboxMessages").InSchema("patient_management");
    }
}
