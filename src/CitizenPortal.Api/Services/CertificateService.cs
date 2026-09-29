using CitizenPortal.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CitizenPortal.Api.Services;

public class CertificateService : ICertificateService
{
    public CertificateService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateCertificatePdf(Application application, Citizen citizen, string serviceName, string departmentName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("NAGRI SEVA CITIZEN PORTAL").FontSize(18).Bold().FontColor("#582160");
                    col.Item().AlignCenter().Text(departmentName).FontSize(12).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#582160");
                });

                page.Content().PaddingVertical(25).Column(col =>
                {
                    col.Item().AlignCenter().Text(serviceName.ToUpperInvariant()).FontSize(16).Bold();
                    col.Item().PaddingTop(20).Text($"This is to certify that the application submitted by:");

                    col.Item().PaddingTop(10).Text(citizen.FullName).FontSize(14).Bold();

                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(180);
                            c.RelativeColumn();
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().Padding(4).Text(label).SemiBold();
                            table.Cell().Padding(4).Text(value);
                        }

                        Row("Application Number", application.ApplicationNumber);
                        Row("Financial Year", application.FinancialYear);
                        Row("Status", application.Status.ToString());
                        Row("Submitted On", application.SubmittedOn.ToString("dd MMM yyyy"));
                        Row("Approved On", application.UpdatedOn.ToString("dd MMM yyyy"));
                        Row("Issued On", DateTime.UtcNow.ToString("dd MMM yyyy"));
                    });

                    col.Item().PaddingTop(25).Text(
                        "This certificate is system-generated and valid only for the financial year mentioned above. " +
                        "It is issued electronically through the Nagri Seva Citizen Portal and does not require a physical signature."
                    ).FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Nagri Seva Citizen Portal — ").FontSize(9);
                    x.Span("This is a reference implementation, not an official government document.").FontSize(9).Italic();
                });
            });
        });

        return document.GeneratePdf();
    }
}
