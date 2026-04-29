using System;
using FluentAssertions;
using HC.LIS.TcpMessage;

namespace HC.LIS.TcpMessage.Tests;

public class SystemExecutionContextAccessorTests
{
    private static readonly SystemExecutionContextAccessor Sut = new();

    [Fact]
    public void UserIdIsWellKnownSystemGuid()
    {
        Sut.UserId.Should().Be(new Guid("00000000-cafe-face-bead-000000000001"));
    }

    [Fact]
    public void UserNameIsTcpMessageSystem()
    {
        Sut.UserName.Should().Be("tcpmessage-system");
    }

    [Fact]
    public void CorrelationIdIsSystem()
    {
        Sut.CorrelationId.Should().Be("system");
    }

    [Fact]
    public void IsAvailableIsTrue()
    {
        Sut.IsAvailable.Should().BeTrue();
    }
}
