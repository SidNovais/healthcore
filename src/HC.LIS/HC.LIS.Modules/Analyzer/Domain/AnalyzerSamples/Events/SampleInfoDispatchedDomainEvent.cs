using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Events;

public class SampleInfoDispatchedDomainEvent(
    Guid analyzerSampleId,
    string sampleBarcode,
    DateTime dispatchedAt
) : DomainEvent
{
    public Guid AnalyzerSampleId { get; } = analyzerSampleId;
    public string SampleBarcode { get; } = sampleBarcode;
    public DateTime DispatchedAt { get; } = dispatchedAt;
}
