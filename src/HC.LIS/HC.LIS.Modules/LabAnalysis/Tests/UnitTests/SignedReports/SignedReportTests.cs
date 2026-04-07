using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Events;
using HC.LIS.Modules.LabAnalysis.Domain.SignedReports.Rules;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems;
using HC.LIS.Modules.LabAnalysis.Domain.WorklistItems.Rules;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.SignedReports;

public class SignedReportTests : TestBase
{
    [Fact]
    public void CreateSignedReportIsSuccessful()
    {
        SignedReport report = SignedReportFactory.Create();

        SignedReportCreatedDomainEvent evt = AssertPublishedDomainEvent<SignedReportCreatedDomainEvent>(report);
        evt.ReportId.Should().Be(SignedReportSampleData.ReportId);
        evt.WorklistItemId.Should().Be(SignedReportSampleData.WorklistItemId);
        evt.OrderId.Should().Be(SignedReportSampleData.OrderId);
        evt.OrderItemId.Should().Be(SignedReportSampleData.OrderItemId);
        evt.Signature.Should().Be(SignedReportSampleData.Signature);
        evt.SignedBy.Should().Be(SignedReportSampleData.SignedBy);
        evt.CreatedAt.Should().Be(SignedReportSampleData.CreatedAt);
        evt.AnalyteSnapshots.Should().BeEquivalentTo(SignedReportSampleData.AnalyteSnapshots);
    }

    [Fact]
    public void HtmlUploadedIsSuccessful()
    {
        SignedReport report = SignedReportFactory.Create();
        report.HtmlUploaded(SignedReportSampleData.HtmlReportPath, SignedReportSampleData.HtmlUploadedAt);

        HtmlReportUploadedDomainEvent evt = AssertPublishedDomainEvent<HtmlReportUploadedDomainEvent>(report);
        evt.ReportId.Should().Be(SignedReportSampleData.ReportId);
        evt.WorklistItemId.Should().Be(SignedReportSampleData.WorklistItemId);
        evt.HtmlReportPath.Should().Be(SignedReportSampleData.HtmlReportPath);
        evt.UploadedAt.Should().Be(SignedReportSampleData.HtmlUploadedAt);
    }

    [Fact]
    public void PdfUploadedIsSuccessful()
    {
        SignedReport report = SignedReportFactory.CreateWithHtml();
        report.PdfUploaded(SignedReportSampleData.PdfReportPath, SignedReportSampleData.PdfUploadedAt);

        PdfReportUploadedDomainEvent evt = AssertPublishedDomainEvent<PdfReportUploadedDomainEvent>(report);
        evt.ReportId.Should().Be(SignedReportSampleData.ReportId);
        evt.WorklistItemId.Should().Be(SignedReportSampleData.WorklistItemId);
        evt.PdfReportPath.Should().Be(SignedReportSampleData.PdfReportPath);
        evt.UploadedAt.Should().Be(SignedReportSampleData.PdfUploadedAt);
    }

    [Fact]
    public void HtmlUploadedThrowsWhenAlreadyUploaded()
    {
        SignedReport report = SignedReportFactory.CreateWithHtml();

        AssertBrokenRule<CannotUploadHtmlWhenAlreadyUploadedRule>(() =>
            report.HtmlUploaded(SignedReportSampleData.HtmlReportPath, SignedReportSampleData.HtmlUploadedAt));
    }

    [Fact]
    public void PdfUploadedThrowsWhenHtmlNotUploaded()
    {
        SignedReport report = SignedReportFactory.Create();

        AssertBrokenRule<CannotUploadPdfWithoutHtmlRule>(() =>
            report.PdfUploaded(SignedReportSampleData.PdfReportPath, SignedReportSampleData.PdfUploadedAt));
    }

    [Fact]
    public void PdfUploadedThrowsWhenAlreadyUploaded()
    {
        SignedReport report = SignedReportFactory.CreateWithHtmlAndPdf();

        AssertBrokenRule<CannotUploadPdfWhenAlreadyUploadedRule>(() =>
            report.PdfUploaded(SignedReportSampleData.PdfReportPath, SignedReportSampleData.PdfUploadedAt));
    }

    [Fact]
    public void CannotSignReportWhenWorklistItemIsNotInReportGeneratedStatus()
    {
        AssertBrokenRule<CannotSignReportWhenNotInReportGeneratedStatusRule>(() =>
            SignedReport.Create(
                SignedReportSampleData.ReportId,
                WorklistItemForSigning.From(
                    SignedReportSampleData.WorklistItemId,
                    SignedReportSampleData.OrderId,
                    SignedReportSampleData.OrderItemId,
                    WorklistItemStatus.Pending,
                    []),
                SignedReportSampleData.Signature,
                SignedReportSampleData.SignedBy,
                SignedReportSampleData.CreatedAt));
    }
}
