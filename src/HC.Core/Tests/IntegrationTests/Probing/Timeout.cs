using System;

namespace HC.Core.IntegrationTests.Probing;

public class Timeout(int duration)
{
    private readonly DateTime _endTime = DateTime.Now.AddMilliseconds(duration);
    public bool HasTimedOut() => DateTime.Now > _endTime;
}
