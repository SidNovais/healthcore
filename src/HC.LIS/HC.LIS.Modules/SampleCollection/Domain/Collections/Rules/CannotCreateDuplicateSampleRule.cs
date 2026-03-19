using System;
using System.Collections.Generic;
using System.Linq;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections.Rules;

public class CreateDuplicateSampleException : BaseBusinessRuleException
{
    public CreateDuplicateSampleException(string message) : base(message) { }
    public CreateDuplicateSampleException(string message, System.Exception innerException) : base(message, innerException) { }
    public CreateDuplicateSampleException(IBusinessRule rule) : base(rule) { }
    public CreateDuplicateSampleException() { }
}

public class CannotCreateDuplicateSampleRule(
    IEnumerable<Sample> samples,
    Guid sampleId
) : IBusinessRule
{
    public bool IsBroken() => samples.Any(s => s.SampleId.Value == sampleId);
    public void ThrowException() => throw new CreateDuplicateSampleException(this);
    public string Message => "Cannot create a sample with a duplicate identifier";
}
