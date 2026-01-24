using System;

namespace HC.Core.IntegrationTests.Probing;

public class AssertErrorException : Exception
{
    public AssertErrorException() { }

    public AssertErrorException(string message) : base(message) { }

    public AssertErrorException(string message, Exception innerException) : base(message, innerException) { }
}
