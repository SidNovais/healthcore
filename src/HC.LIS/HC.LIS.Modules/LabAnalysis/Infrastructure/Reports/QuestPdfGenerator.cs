using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HC.LIS.Modules.LabAnalysis.Application.Contracts;
using HC.LIS.Modules.LabAnalysis.Application.WorklistItems.GetWorklistItemDetails;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Reports;

internal class QuestPdfGenerator : IPdfGenerator
{
    public Task<byte[]> GenerateAsync(
        WorklistItemDetailsDto worklistItemDetails,
        string signature,
        Guid signedBy,
        DateTime signedAt,
        CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        byte[] pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(header =>
                {
                    header.Item().Text("Laboratory Report")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    header.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(15).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Exam Code").SemiBold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(worklistItemDetails.ExamCode);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Patient ID").SemiBold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(worklistItemDetails.PatientId.ToString());
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Sample Barcode").SemiBold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(worklistItemDetails.SampleBarcode);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Report Date").SemiBold().FontColor(Colors.Grey.Darken2);
                            c.Item().Text(signedAt.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " UTC");
                        });
                    });

                    column.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            static IContainer HeaderStyle(IContainer c) =>
                                c.Background(Colors.Blue.Darken2).Padding(5);

                            header.Cell().Element(HeaderStyle).Text("Analyte").SemiBold().FontColor(Colors.White);
                            header.Cell().Element(HeaderStyle).Text("Result").SemiBold().FontColor(Colors.White);
                            header.Cell().Element(HeaderStyle).Text("Unit").SemiBold().FontColor(Colors.White);
                            header.Cell().Element(HeaderStyle).Text("Reference Range").SemiBold().FontColor(Colors.White);
                            header.Cell().Element(HeaderStyle).Text("Status").SemiBold().FontColor(Colors.White);
                        });

                        bool isAlternate = false;
                        foreach (var result in worklistItemDetails.AnalyteResults.OrderBy(r => r.AnalyteCode))
                        {
                            string bg = isAlternate ? Colors.Grey.Lighten4 : Colors.White;
                            string statusColor = result.IsOutOfRange ? Colors.Red.Darken2 : Colors.Green.Darken2;
                            string statusLabel = result.IsOutOfRange ? "Abnormal" : "Normal";

                            IContainer CellStyle(IContainer c) => c.Background(bg).Padding(5);

                            table.Cell().Element(CellStyle).Text(result.AnalyteCode);
                            table.Cell().Element(CellStyle).Text(result.ResultValue).FontColor(statusColor);
                            table.Cell().Element(CellStyle).Text(result.ResultUnit);
                            table.Cell().Element(CellStyle).Text(result.ReferenceRange);
                            table.Cell().Element(CellStyle).Text(statusLabel).SemiBold().FontColor(statusColor);

                            isAlternate = !isAlternate;
                        }
                    });

                    column.Item().PaddingTop(30).BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10).Column(footer =>
                    {
                        footer.Item().Text($"Signed by: {signedBy}").FontSize(10).FontColor(Colors.Grey.Darken2);
                        footer.Item().Text($"Signature: {signature}").FontSize(10);
                        footer.Item().Text($"Date: {signedAt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)} UTC").FontSize(10);
                        footer.Item().PaddingTop(5).Text("This is an official laboratory report. The content of this document is final and immutable.")
                            .FontSize(8).FontColor(Colors.Grey.Medium).Italic();
                    });
                });
            });
        }).GeneratePdf();

        return Task.FromResult(pdfBytes);
    }
}
