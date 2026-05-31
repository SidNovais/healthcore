using FluentMigrator;

namespace HC.LIS.Database.PatientManagement;

[Migration(20260530120000)]
public class PatientManagementModuleAddSchema : Migration
{
    public override void Up()
    {
        Create.Schema("patient_management");
    }

    public override void Down()
    {
        Delete.Schema("patient_management");
    }
}
