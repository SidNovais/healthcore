using Newtonsoft.Json;
using HC.LIS.Modules.SampleCollection.Application.Configuration.Commands;

namespace HC.LIS.Modules.SampleCollection.Application.Collections.GenerateSampleBarcodes;

[method: JsonConstructor]
public class GenerateSampleBarcodesForCollectionRequestCommand(
    Guid id,
    Guid collectionRequestId
) : InternalCommandBase(id)
{
    public Guid CollectionRequestId { get; } = collectionRequestId;
}
