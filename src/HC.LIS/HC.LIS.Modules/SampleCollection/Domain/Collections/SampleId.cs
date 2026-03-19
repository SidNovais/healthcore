using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Collections;

public class SampleId(Guid value) : Id(value);
