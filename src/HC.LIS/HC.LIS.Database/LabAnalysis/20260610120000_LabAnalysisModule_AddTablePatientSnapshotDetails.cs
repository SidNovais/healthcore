using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260610120000)]
public class LabAnalysisModuleAddTablePatientSnapshotDetails : Migration
{
    public override void Up()
    {
        Create.Table("PatientSnapshotDetails").InSchema("lab_analysis")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("FullName").AsString(255).NotNullable()
            .WithColumn("DateOfBirth").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("Gender").AsString(50).Nullable()
            .WithColumn("MothersFullName").AsString(255).Nullable()
            .WithColumn("DocumentId").AsString(100).Nullable()
            .WithColumn("Phone").AsString(50).Nullable()
            .WithColumn("Email").AsString(255).Nullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("RegisteredAt").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("AnonymizedAt").AsCustom("TIMESTAMPTZ").Nullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("PatientSnapshotDetails").InSchema("lab_analysis");
    }
}
