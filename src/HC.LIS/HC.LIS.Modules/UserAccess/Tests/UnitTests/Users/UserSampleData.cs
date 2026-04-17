using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.UserAccess.UnitTests.Users;

public readonly struct UserSampleData
{
    public static readonly Guid UserId = Guid.Parse("019e1a2b-3c4d-7e5f-a6b7-c8d9e0f1a2b3");
    public static readonly Guid CreatedById = Guid.Parse("019e1a2b-3c4d-7e5f-a6b7-c8d9e0f1a2b4");
    public const string Email = "jane.doe@hclis.local";
    public const string FullName = "Jane Doe";
    public static readonly DateTime Birthdate = new(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    public const string Gender = "Female";
    public const string Role = "LabTechnician";
    public const string InvitationToken = "tok-test-abc123";
    public static readonly DateTime CreatedAt = SystemClock.Now;
}
