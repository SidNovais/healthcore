using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;

namespace HC.LIS.Modules.LabAnalysis.Application.Reports;

internal static class HtmlReportTemplate
{
    public static string Generate(
        WorklistItemDetailsDto dto,
        string signature,
        Guid signedBy,
        DateTime signedAt)
    {
        var sb = new StringBuilder();
        sb.Append("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>Laboratory Report</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 40px; color: #222; }
                    h1 { color: #1a4a7a; }
                    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                    th { background-color: #1a4a7a; color: white; padding: 10px; text-align: left; }
                    td { padding: 8px 10px; border-bottom: 1px solid #ddd; }
                    tr:nth-child(even) { background-color: #f5f5f5; }
                    .abnormal { color: #c0392b; font-weight: bold; }
                    .footer { margin-top: 40px; border-top: 1px solid #ccc; padding-top: 20px; font-size: 0.9em; }
                    .header-info { display: flex; gap: 40px; margin-bottom: 20px; }
                    .header-info div { flex: 1; }
                    label { font-weight: bold; display: block; color: #555; }
                </style>
            </head>
            <body>
                <h1>Laboratory Report</h1>
                <div class="header-info">
            """);

        sb.Append(CultureInfo.InvariantCulture, $"""
                    <div>
                        <label>Exam Code</label><span>{EscapeHtml(dto.ExamCode)}</span>
                    </div>
                    <div>
                        <label>Patient ID</label><span>{dto.PatientId}</span>
                    </div>
                    <div>
                        <label>Sample Barcode</label><span>{EscapeHtml(dto.SampleBarcode)}</span>
                    </div>
                    <div>
                        <label>Report Date</label><span>{signedAt:yyyy-MM-dd HH:mm:ss} UTC</span>
                    </div>
                </div>
                <table>
                    <thead>
                        <tr>
                            <th>Analyte</th>
                            <th>Result</th>
                            <th>Unit</th>
                            <th>Reference Range</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
            """);

        foreach (var result in dto.AnalyteResults.OrderBy(r => r.AnalyteCode))
        {
            string statusLabel = result.IsOutOfRange ? "Abnormal" : "Normal";
            string statusClass = result.IsOutOfRange ? "abnormal" : string.Empty;

            sb.Append(CultureInfo.InvariantCulture, $"""
                        <tr>
                            <td>{EscapeHtml(result.AnalyteCode)}</td>
                            <td class="{statusClass}">{EscapeHtml(result.ResultValue)}</td>
                            <td>{EscapeHtml(result.ResultUnit)}</td>
                            <td>{EscapeHtml(result.ReferenceRange)}</td>
                            <td class="{statusClass}">{statusLabel}</td>
                        </tr>
                """);
        }

        sb.Append(CultureInfo.InvariantCulture, $"""
                    </tbody>
                </table>
                <div class="footer">
                    <p><strong>Signed by:</strong> {signedBy}</p>
                    <p><strong>Signature:</strong> {EscapeHtml(signature)}</p>
                    <p><strong>Date:</strong> {signedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <p style="font-size:0.8em;color:#888;">This is an official laboratory report. The content of this document is final and immutable.</p>
                </div>
            </body>
            </html>
            """);

        return sb.ToString();
    }

    private static string EscapeHtml(string value) => WebUtility.HtmlEncode(value);
}
