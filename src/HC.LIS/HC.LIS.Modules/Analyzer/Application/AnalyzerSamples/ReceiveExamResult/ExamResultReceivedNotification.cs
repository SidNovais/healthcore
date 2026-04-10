using HC.Core.Application.Events;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ReceiveExamResult;

public class ExamResultReceivedNotification(ExamResultReceivedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<ExamResultReceivedDomainEvent>(domainEvent, id)
{
}
