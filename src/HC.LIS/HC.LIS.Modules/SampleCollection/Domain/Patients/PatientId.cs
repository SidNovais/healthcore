using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.SampleCollection.Domain.Patients;

public class PatientId(Guid value) : Id(value);
