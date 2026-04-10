using HC.Core.Application.Events;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

public class AnalyzerSampleCreatedNotification(AnalyzerSampleCreatedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<AnalyzerSampleCreatedDomainEvent>(domainEvent, id)
{
}
