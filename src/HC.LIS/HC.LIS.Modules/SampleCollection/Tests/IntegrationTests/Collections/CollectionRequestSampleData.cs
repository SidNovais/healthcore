using System;

namespace HC.LIS.Modules.SampleCollection.IntegrationTests.Collections;

public readonly struct CollectionRequestSampleData
{
    public static readonly Guid CollectionRequestId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e40");
    public static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550c0");
    public static readonly Guid OrderId = Guid.Parse("019b664c-79f0-7f45-87f3-84664a00e635");
    public static readonly Guid ExamId = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5026");
    public static readonly Guid TechnicianId = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5027");
    public static readonly string TubeType = "EDTA";
    public static readonly string BarcodeValue = "BC-001";
    public static readonly DateTime ArrivedAt = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime WaitingAt = new(2026, 1, 1, 8, 5, 0, DateTimeKind.Utc);
    public static readonly DateTime CalledAt = new(2026, 1, 1, 8, 10, 0, DateTimeKind.Utc);
    public static readonly DateTime BarcodeCreatedAt = new(2026, 1, 1, 8, 15, 0, DateTimeKind.Utc);
    public static readonly DateTime CollectedAt = new(2026, 1, 1, 8, 20, 0, DateTimeKind.Utc);
}
