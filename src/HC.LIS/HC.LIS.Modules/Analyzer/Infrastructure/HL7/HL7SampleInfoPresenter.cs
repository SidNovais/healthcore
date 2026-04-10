using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Modules.Analyzer.Application.Contracts;

namespace HC.LIS.Modules.Analyzer.Infrastructure.HL7;

internal class HL7SampleInfoPresenter : ISampleInfoPresenter
{
    public string Format(SampleInfoDto sampleInfo)
    {
        // Simulated HL7 v2.x QBP response — not spec-complete
        var exams = string.Join("~", sampleInfo.Exams.Select(e => e.ExamMnemonic));
        return $"MSH|^~\\&|LIS|||{DateTime.UtcNow:yyyyMMddHHmmss}||RSP^K11|{Guid.NewGuid()}|P|2.5\r" +
               $"PID|||{sampleInfo.Id}||{sampleInfo.PatientName}||{sampleInfo.PatientBirthdate:yyyyMMdd}|{sampleInfo.PatientGender}\r" +
               $"SPM|||{sampleInfo.SampleBarcode}|^\r" +
               $"OBR|||{sampleInfo.SampleBarcode}|{exams}\r";
    }
}
