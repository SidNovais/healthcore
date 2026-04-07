using System;
using HC.Core.Domain.EventSourcing;

namespace HC.LIS.Modules.LabAnalysis.Domain.SignedReports;

public class SignedReportId(Guid id) : AggregateId<SignedReport>(id);
