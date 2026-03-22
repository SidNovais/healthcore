using HC.LIS.Modules.SampleCollection.Application.Contracts;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GetSampleDetails;

public class GetSampleDetailsQuery(
    Guid sampleId
) : QueryBase<SampleDetailsDto?>
{
    public Guid SampleId { get; } = sampleId;
}
