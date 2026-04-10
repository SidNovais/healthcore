using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

public class AnalyzerSampleExam : Entity
{
    internal AnalyzerSampleExamId? AnalyzerSampleExamId { get; private set; }
    internal string ExamMnemonic { get; private set; } = string.Empty;
    internal bool ResultReceived { get; private set; }

    private AnalyzerSampleExam() { }

    internal static AnalyzerSampleExam Create(string examMnemonic)
    {
        return new AnalyzerSampleExam { ExamMnemonic = examMnemonic };
    }

    internal void AssignId(Guid id)
    {
        AnalyzerSampleExamId = new AnalyzerSampleExamId(id);
    }

    internal void MarkResultReceived()
    {
        ResultReceived = true;
    }
}
