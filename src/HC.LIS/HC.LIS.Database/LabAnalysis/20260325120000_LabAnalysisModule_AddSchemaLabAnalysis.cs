using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260325120000)]
public class LabAnalysisModuleAddSchemaLabAnalysis : Migration
{
    public override void Up()
    {
        Create.Schema("lab_analysis");
    }

    public override void Down()
    {
        Delete.Schema("lab_analysis");
    }
}
