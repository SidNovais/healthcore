using HC.Core.Domain.EventSourcing;
using HC.LIS.Modules.Analyzer.Application.Configuration.Commands;
using HC.LIS.Modules.Analyzer.Domain.AnalyzerSamples;

namespace HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.CreateAnalyzerSample;

internal class CreateAnalyzerSampleCommandHandler(
    IAggregateStore aggregateStore
) : ICommandHandler<CreateAnalyzerSampleCommand, Guid>
{
    private readonly IAggregateStore _aggregateStore = aggregateStore;

    public Task<Guid> Handle(
        CreateAnalyzerSampleCommand command,
        CancellationToken cancellationToken
    )
    {
        PatientInfo patientInfo = PatientInfo.Of(
            command.PatientId,
            command.PatientName,
            command.PatientBirthdate,
            command.PatientGender);

        IReadOnlyCollection<ExamInfo> exams = command.Exams
            .Select(e => new ExamInfo(e.ExamId, e.ExamMnemonic))
            .ToList()
            .AsReadOnly();

        AnalyzerSample sample = AnalyzerSample.Create(
            command.AnalyzerSampleId,
            command.SampleId,
            command.SampleBarcode,
            patientInfo,
            exams,
            command.CreatedAt);

        _aggregateStore.Start(sample);
        return Task.FromResult(sample.Id);
    }
}
