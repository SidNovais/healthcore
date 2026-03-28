using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;

public class WorklistItemId(Guid id) : AggregateId<WorklistItem>(id);
