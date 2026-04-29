using System;
using System.Threading.Tasks;
using FluentAssertions;
using HC.Core.Domain;
using HC.Core.IntegrationTests.Probing;

namespace HC.LIS.Tests.IntegrationEvents;

internal static class IntegrationTestAssert
{
    internal static async Task AssertEventually(IProbe probe, int timeoutMs)
        => await new Poller(timeoutMs).CheckAsync(probe);

    internal static void AssertBrokenRule<TRule>(Action testDelegate)
        where TRule : class, IBusinessRule
        => testDelegate.Should()
            .Throw<BaseBusinessRuleException>()
            .Which.Rule.Should().BeOfType<TRule>();
}
