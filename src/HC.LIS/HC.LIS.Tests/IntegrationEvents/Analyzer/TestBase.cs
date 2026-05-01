using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.ForwardRawResult;
using HC.LIS.Modules.Analyzer.Application.AnalyzerSamples.GetSampleInfoByBarcode;
using HC.LIS.Tests.IntegrationEvents.Probes;

namespace HC.LIS.Tests.IntegrationEvents.Analyzer;

[Collection("IntegrationTests")]
public abstract class TestBase : HC.LIS.Tests.IntegrationEvents.SampleCollection.TestBase
{
    protected static byte[] BuildOruR01(
        string barcode,
        string examMnemonic,
        string resultValue = "5.0",
        string unit = "mg/dL",
        string refRange = "3.5-7.0")
    {
        // Build HL7 ORU^R01 — parser splits on \r, reads SPM.3=barcode, OBX.3=mnemonic^Desc, OBX.5-7=result
        string msg =
            $"MSH|^~\\&|Analyzer|Lab|||20260430000000||ORU^R01|1|P|2.5\r" +
            $"SPM|||{barcode}\r" +
            $"OBX|1|NM|{examMnemonic}^Description||{resultValue}|{unit}|{refRange}\r";
        return Encoding.UTF8.GetBytes(msg);
    }

    protected async Task<(Guid orderId, Guid orderItemId, Guid sampleId, string barcode, Guid worklistItemId)>
        SetupExamResultReadyAsync(string barcode, string examMnemonic)
    {
        // Run full Group B chain (SetupCollectedSampleAsync from SampleCollection.TestBase)
        var (orderId, orderItemId, sampleId, _) = await SetupCollectedSampleAsync(barcode, examMnemonic);

        // Wait for two-hop: SampleCollected → WorklistItemCreated → AssignWorklistItem
        await IntegrationTestAssert.AssertEventually(
            new GetWorklistItemAssignedFromAnalyzerProbe(barcode, examMnemonic, AnalyzerModule),
            timeoutMs: 25_000);

        // Retrieve worklistItemId from Analyzer facade (WorklistItemId is populated after assignment)
        var sampleInfo = await AnalyzerModule.ExecuteQueryAsync(new GetSampleInfoByBarcodeQuery(barcode));
        var worklistItemId = sampleInfo!.Exams.Single(e => e.ExamMnemonic == examMnemonic).WorklistItemId!.Value;

        return (orderId, orderItemId, sampleId, barcode, worklistItemId);
    }
}
