using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.UnitTests.Collections;

public readonly struct CollectionRequestSampleData
{
    public static readonly Guid CollectionRequestId = Guid.Parse("019b6642-6c05-7678-919a-2bd510a95e40");
    public static readonly Guid PatientId = Guid.Parse("019b664c-52a4-7f37-a794-6da2481550c0");
    public static readonly Guid OrderId = Guid.Parse("019b664c-79f0-7f45-87f3-84664a00e635");
    public static readonly Guid TechnicianId = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5027");
    public static readonly Guid ExamId1 = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5028");
    public static readonly Guid ExamId2 = Guid.Parse("019b6c5d-fbf9-7e35-aa12-c38922ec5029");
    public static readonly string TubeType = "EDTA";
    public static readonly string BarcodeValue = "BC-001";
    public static readonly bool ExamPreparationVerified = true;
    public static readonly DateTime ArrivedAt = SystemClock.Now;
    public static readonly DateTime WaitingAt = SystemClock.Now;
    public static readonly DateTime CalledAt = SystemClock.Now;
    public static readonly DateTime CreatedAt = SystemClock.Now;
    public static readonly DateTime CollectedAt = SystemClock.Now;
}
