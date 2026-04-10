using HC.Core.Application.Events;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.DispatchSampleInfo;

public class SampleInfoDispatchedNotification(SampleInfoDispatchedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<SampleInfoDispatchedDomainEvent>(domainEvent, id)
{
}
