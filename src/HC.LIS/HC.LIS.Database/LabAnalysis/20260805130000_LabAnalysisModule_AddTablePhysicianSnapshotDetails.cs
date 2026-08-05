using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260805130000)]
public class LabAnalysisModuleAddTablePhysicianSnapshotDetails : Migration
{
    public override void Up()
    {
        Create.Table("PhysicianSnapshotDetails").InSchema("lab_analysis")
            .WithColumn("Id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("FullName").AsString(255).NotNullable()
            .WithColumn("LicenceNumber").AsString(100).Nullable()
            .WithColumn("Status").AsString(50).NotNullable()
            .WithColumn("RegisteredAt").AsCustom("TIMESTAMPTZ").NotNullable()
            .WithColumn("UpdatedAt").AsCustom("TIMESTAMPTZ").Nullable()
            .WithColumn("DeactivatedAt").AsCustom("TIMESTAMPTZ").Nullable()
        ;
    }

    public override void Down()
    {
        Delete.Table("PhysicianSnapshotDetails").InSchema("lab_analysis");
    }
}
