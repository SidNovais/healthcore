using System;

namespace HC.LIS.Modules.LabAnalysis.Application.SignedReports.GetSignedReportDetails;

public class SignedReportDetailsDto
{
    public Guid Id { get; set; }
    public Guid WorklistItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }
    public string? HtmlReportPath { get; set; }
    public string? PdfReportPath { get; set; }
    public string Signature { get; set; } = string.Empty;
    public Guid SignedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
