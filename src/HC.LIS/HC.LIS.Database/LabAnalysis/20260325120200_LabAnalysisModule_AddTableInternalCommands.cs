using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260325120200)]
public class LabAnalysisModuleAddTableInternalCommands : Migration
{
    public override void Up()
    {
        Create.Table("InternalCommands").InSchema("lab_analysis")
          .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
          .WithColumn("EnqueueDate").AsCustom("TIMESTAMPTZ").NotNullable()
          .WithColumn("Type").AsString(255).NotNullable().Indexed()
          .WithColumn("Data").AsString(int.MaxValue).NotNullable()
          .WithColumn("ProcessedDate").AsCustom("TIMESTAMPTZ").Nullable().Indexed()
          .WithColumn("Error").AsString(int.MaxValue).Nullable()
        ;
        Create.Index()
          .OnTable("InternalCommands")
          .InSchema("lab_analysis")
          .OnColumn("EnqueueDate")
          .Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("InternalCommands").InSchema("lab_analysis");
    }
}
