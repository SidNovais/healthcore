using System;
using FluentAssertions;
using HC.LIS.Modules.LabAnalysis.Application.Reports;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using Xunit;

namespace HC.LIS.Modules.LabAnalysis.UnitTests.Reports;

public class HtmlReportTemplateTests
{
    private static readonly Guid SignedBy = Guid.Parse("019b664c-52a4-7f37-a794-000000000301");
    private static readonly DateTime SignedAt = new(2026, 8, 5, 16, 0, 0, DateTimeKind.Utc);

    private static WorklistItemDetailsDto DtoWithPhysician(string? requestedByName) => new()
    {
        Id = Guid.CreateVersion7(),
        SampleBarcode = "SC-RPT-001",
        ExamCode = "GLU",
        PatientId = Guid.Parse("019b664c-52a4-7f37-a794-000000000302"),
        RequestedByName = requestedByName,
        Status = "Completed"
    };

    [Fact]
    public void HeaderCarriesTheRequestingPhysician()
    {
        string html = HtmlReportTemplate.Generate(
            DtoWithPhysician("Ana Lima"), "sig", SignedBy, SignedAt);

        html.Should().Contain("Requesting Physician");
        html.Should().Contain("Ana Lima");
    }

    [Fact]
    public void HeaderFallsBackWhenTheOrderHasNoPhysicianMapping()
    {
        string html = HtmlReportTemplate.Generate(
            DtoWithPhysician(null), "sig", SignedBy, SignedAt);

        html.Should().Contain("Requesting Physician");
        html.Should().Contain("Unknown physician");
    }

    [Fact]
    public void PhysicianNameIsHtmlEscaped()
    {
        string html = HtmlReportTemplate.Generate(
            DtoWithPhysician("<script>alert(1)</script>"), "sig", SignedBy, SignedAt);

        html.Should().NotContain("<script>");
        html.Should().Contain("&lt;script&gt;");
    }
}
