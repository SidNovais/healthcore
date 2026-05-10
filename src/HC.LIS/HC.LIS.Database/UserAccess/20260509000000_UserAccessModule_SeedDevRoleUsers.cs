using System;
using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260509000000)]
public class UserAccessModuleSeedDevRoleUsers : Migration
{
    private static readonly Guid ReceptionistId = new("00000000-0000-0000-0000-000000000002");
    private static readonly Guid LabTechnicianId = new("00000000-0000-0000-0000-000000000003");
    private static readonly Guid PhysicianId     = new("00000000-0000-0000-0000-000000000004");
    private static readonly Guid RootUserId      = new("00000000-0000-0000-0000-000000000001");

    // PBKDF2/SHA-256/310000 hash of "Admin1234!" — same as root seed user.
    private const string DevPasswordHash =
        "ajuPEkXNngFy9FaJq8Pedw==:PD9/df05aHLn66O4f7Fwjn12vu4yEUAJ4bfjh5Igq8g=";

    public override void Up()
    {
        Insert.IntoTable("users").InSchema("user_access").Row(new
        {
            id               = ReceptionistId,
            email            = "receptionist@hclis.local",
            full_name        = "Dev Receptionist",
            birthdate        = new DateTime(1990, 1, 1),
            gender           = "Unknown",
            role             = "Receptionist",
            status           = "Active",
            password_hash    = DevPasswordHash,
            invitation_token = (string?)null,
            created_at       = DateTime.UtcNow,
            created_by_id    = RootUserId,
            activated_at     = DateTime.UtcNow,
        });

        Insert.IntoTable("users").InSchema("user_access").Row(new
        {
            id               = LabTechnicianId,
            email            = "labtech@hclis.local",
            full_name        = "Dev Lab Technician",
            birthdate        = new DateTime(1990, 1, 1),
            gender           = "Unknown",
            role             = "LabTechnician",
            status           = "Active",
            password_hash    = DevPasswordHash,
            invitation_token = (string?)null,
            created_at       = DateTime.UtcNow,
            created_by_id    = RootUserId,
            activated_at     = DateTime.UtcNow,
        });

        Insert.IntoTable("users").InSchema("user_access").Row(new
        {
            id               = PhysicianId,
            email            = "physician@hclis.local",
            full_name        = "Dev Physician",
            birthdate        = new DateTime(1990, 1, 1),
            gender           = "Unknown",
            role             = "Physician",
            status           = "Active",
            password_hash    = DevPasswordHash,
            invitation_token = (string?)null,
            created_at       = DateTime.UtcNow,
            created_by_id    = RootUserId,
            activated_at     = DateTime.UtcNow,
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").InSchema("user_access").Row(new { id = PhysicianId });
        Delete.FromTable("users").InSchema("user_access").Row(new { id = LabTechnicianId });
        Delete.FromTable("users").InSchema("user_access").Row(new { id = ReceptionistId });
    }
}
