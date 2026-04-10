using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;

namespace HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples.Rules;

public class ExamMustExistInSampleException : BaseBusinessRuleException
{
    public ExamMustExistInSampleException() { }
    public ExamMustExistInSampleException(string message) : base(message) { }
    public ExamMustExistInSampleException(string message, System.Exception innerException) : base(message, innerException) { }
    public ExamMustExistInSampleException(IBusinessRule rule) : base(rule) { }
}

public class ExamMustExistInSampleRule(
    IList<AnalyzerSampleExam> exams,
    string examMnemonic
) : IBusinessRule
{
    private readonly IList<AnalyzerSampleExam> _exams = exams;
    private readonly string _examMnemonic = examMnemonic;
    public bool IsBroken() => !_exams.Any(e => e.ExamMnemonic == _examMnemonic);
    public void ThrowException() => throw new ExamMustExistInSampleException(this);
    public string Message => $"Exam '{_examMnemonic}' does not exist in the sample";
}
