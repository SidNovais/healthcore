using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class CollectionRequestId(Guid id) : AggregateId<CollectionRequest>(id);
