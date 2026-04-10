using FluentMigrator;

namespace HC.LIS.Database.Analyzer;

[Migration(20260409120000)]
public class AnalyzerModuleAddSchemaAnalyzer : Migration
{
    public override void Up()
    {
        Create.Schema("analyzer");
    }

    public override void Down()
    {
        Delete.Schema("analyzer");
    }
}
