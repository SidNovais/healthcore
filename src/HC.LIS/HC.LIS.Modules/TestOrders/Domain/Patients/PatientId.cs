using System;
using HC.Core.Domain;

namespace HC.LIS.Modules.TestOrders.Domain.Patients;

public class PatientId(Guid value) : Id(value) { }
