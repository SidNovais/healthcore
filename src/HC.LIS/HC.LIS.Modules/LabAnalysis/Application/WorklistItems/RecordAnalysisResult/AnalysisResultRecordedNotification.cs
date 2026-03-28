using System;
using HC.Core.Application.Events;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Events;

namespace HC.LIS.Modules.LabAnalysis.Application.WorklistItems.RecordAnalysisResult;

public class AnalysisResultRecordedNotification(AnalysisResultRecordedDomainEvent domainEvent, Guid id)
    : DomainNotificationBase<AnalysisResultRecordedDomainEvent>(domainEvent, id)
{
}
