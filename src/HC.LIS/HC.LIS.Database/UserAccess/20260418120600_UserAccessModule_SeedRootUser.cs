using System;
using FluentMigrator;

namespace HC.LIS.Database.UserAccess;

[Migration(20260418120600)]
public class UserAccessModuleSeedRootUser : Migration
{
    private static readonly Guid RootUserId = new("00000000-0000-0000-0000-000000000001");

    // PBKDF2/SHA-256/310000 hash of "Admin1234!" with fixed salt.
    // Override in production via ASPNETCORE_HCLIS_ROOT_PASSWORD_HASH env var.
    private const string DevPasswordHash =
        "ajuPEkXNngFy9FaJq8Pedw==:PD9/df05aHLn66O4f7Fwjn12vu4yEUAJ4bfjh5Igq8g=";

    public override void Up()
    {
        string passwordHash =
            Environment.GetEnvironmentVariable("ASPNETCORE_HCLIS_ROOT_PASSWORD_HASH")
            ?? DevPasswordHash;

        Insert.IntoTable("users").InSchema("user_access").Row(new
        {
            id = RootUserId,
            email = "root@hclis.local",
            full_name = "Root Administrator",
            birthdate = new DateTime(1990, 1, 1),
            gender = "Unknown",
            role = "ITAdmin",
            status = "Active",
            password_hash = passwordHash,
            invitation_token = (string?)null,
            created_at = DateTime.UtcNow,
            created_by_id = (Guid?)null,
            activated_at = DateTime.UtcNow
        });
    }

    public override void Down()
    {
        Delete.FromTable("users").InSchema("user_access")
            .Row(new { id = RootUserId });
    }
}
