using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

internal class CreateAnalyzerSampleBySampleCollectedCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CreateAnalyzerSampleBySampleCollectedCommand>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public Task Handle(
        CreateAnalyzerSampleBySampleCollectedCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid analyzerSampleId = DeriveAnalyzerSampleId(command.SampleId);

        PatientInfo patientInfo = PatientInfo.Of(
            command.PatientId,
            command.PatientName,
            command.PatientBirthdate,
            command.PatientGender);

        System.Collections.Generic.IReadOnlyCollection<ExamInfo> exams = command.Exams
            .Select(e => new ExamInfo(e.ExamId, e.ExamMnemonic))
            .ToList()
            .AsReadOnly();

        AnalyzerSample sample = AnalyzerSample.Create(
            analyzerSampleId,
            command.SampleId,
            command.SampleBarcode,
            patientInfo,
            exams,
            command.CreatedAt);

        _aggregateStore.Start(sample);
        return Task.CompletedTask;
    }

    private static Guid DeriveAnalyzerSampleId(Guid sampleId)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(sampleId.ToString());
        byte[] hash = SHA256.HashData(inputBytes);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50); // version 5
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // RFC 4122 variant
        return new Guid(hash[..16]);
    }
}
