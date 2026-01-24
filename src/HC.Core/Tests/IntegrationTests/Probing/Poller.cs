using System.Threading;
using System.Threading.Tasks;

namespace HC.Core.IntegrationTests.Probing;

public class Poller(int timeoutMillis)
{
    private readonly int _timeoutMillis = timeoutMillis;
    private readonly int _pollDelayMillis = 1000;
    public async Task CheckAsync(IProbe probe)
    {
        var timeout = new Timeout(_timeoutMillis);
        while (!probe.IsSatisfied())
        {
            if (timeout.HasTimedOut()) throw new AssertErrorException(DescribeFailureOf(probe));
            await Task.Delay(_pollDelayMillis).ConfigureAwait(false);
            await probe.SampleAsync().ConfigureAwait(false);
        }
    }

    public async Task<T?> GetAsync<T>(IProbe<T> probe)
        where T : class
    {
        var timeout = new Timeout(_timeoutMillis);
        T? sample = null;
        while (!probe.IsSatisfied(sample))
        {
            if (timeout.HasTimedOut()) throw new AssertErrorException(DescribeFailureOf(probe));
            await Task.Delay(_pollDelayMillis).ConfigureAwait(false);
            sample = await probe.GetSampleAsync().ConfigureAwait(false);
        }
        return sample;
    }
    private static string DescribeFailureOf(IProbe probe)
      => probe.DescribeFailureTo();
    private static string DescribeFailureOf<T>(IProbe<T> probe)
      => probe.DescribeFailureTo();
}
