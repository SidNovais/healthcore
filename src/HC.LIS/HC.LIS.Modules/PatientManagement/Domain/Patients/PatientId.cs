using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.PatientManagement.Domain.Patients;

public class PatientId(Guid id) : AggregateId<Patient>(id);
