using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.IntegrationTests.Users;

public readonly struct UserSampleData
{
    public static readonly Guid UserId       = Guid.Parse("019e1a00-0000-7000-0000-000000000001");
    public static readonly Guid AdminUserId  = Guid.Parse("019e1a00-0000-7000-0000-000000000002");
    public static readonly Guid CreatedById  = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public const string Email           = "alice@hclis.local";
    public const string FullName        = "Alice Smith";
    public const string Gender          = "Female";
    public const string Role            = "LabTechnician";
    public const string NewRole         = "Physician";
    public const string InvitationToken = "test-invitation-token-abc123";

    // PBKDF2/SHA-256 hash of Password using fixed salt. Matches PasswordHasher implementation.
    public const string Password     = "Admin1234!";
    public const string PasswordHash = "ajuPEkXNngFy9FaJq8Pedw==:PD9/df05aHLn66O4f7Fwjn12vu4yEUAJ4bfjh5Igq8g=";

    public static DateTime Birthdate => new(1990, 6, 15);
}
