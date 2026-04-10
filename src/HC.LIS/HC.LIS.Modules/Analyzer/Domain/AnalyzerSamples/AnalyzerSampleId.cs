using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

public class AnalyzerSampleId(Guid id) : AggregateId<AnalyzerSample>(id);
