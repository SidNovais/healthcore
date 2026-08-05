using FluentMigrator;

namespace HC.LIS.Database.LabAnalysis;

[Migration(20260805130100)]
public class LabAnalysisModuleAddTableOrderPhysicianSnapshotDetails : Migration
{
    public override void Up()
    {
        Create.Table("OrderPhysicianSnapshotDetails").InSchema("lab_analysis")
            .WithColumn("OrderId").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("PhysicianId").AsGuid().NotNullable()
            .WithColumn("RequestedAt").AsCustom("TIMESTAMPTZ").NotNullable()
        ;

        Create.Index("IX_OrderPhysicianSnapshotDetails_PhysicianId")
            .OnTable("OrderPhysicianSnapshotDetails").InSchema("lab_analysis")
            .OnColumn("PhysicianId").Ascending()
        ;
    }

    public override void Down()
    {
        Delete.Table("OrderPhysicianSnapshotDetails").InSchema("lab_analysis");
    }
}
