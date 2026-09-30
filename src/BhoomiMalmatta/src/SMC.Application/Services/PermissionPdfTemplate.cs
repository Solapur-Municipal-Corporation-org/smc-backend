using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMC.Domain.Entities;

namespace SMC.Application.Services;

/// <summary>Single standard server-side template used for every approved permission.</summary>
public static class PermissionPdfTemplate
{
    public static byte[] Render(DemandApplicationWorkflow workflow)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var app = workflow.DemandApplication;
        return QuestPDF.Fluent.Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(28);
            page.DefaultTextStyle(style => style.FontSize(9).FontFamily("Nirmala UI"));
            page.Header().AlignCenter().Column(c => { c.Item().Text("सोलापूर महानगरपालिका").FontSize(18).Bold(); c.Item().Text("भूमी व मालमत्ता व्यवस्थापन विभाग").FontSize(11); c.Item().PaddingTop(7).Text("परवाना / परवानगी पत्र").FontSize(16).Bold(); c.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Blue.Darken2); });
            page.Content().PaddingVertical(12).Column(c =>
            {
                void Row(string label, string? value) => c.Item().PaddingVertical(1).Row(r => { r.RelativeItem(2).Text(label).Bold(); r.RelativeItem(3).Text(string.IsNullOrWhiteSpace(value) ? "-" : value); });
                void Section(string title) => c.Item().PaddingTop(7).Background(Colors.Blue.Lighten5).Padding(5).Text(title).Bold().FontColor(Colors.Blue.Darken3);
                Section("मंजूर अर्जाचा तपशील");
                string Measure(decimal? value, string unit) => value.HasValue ? $"{value.Value.ToString("N2", CultureInfo.InvariantCulture)} {unit}" : "-";
                Row("अर्ज क्रमांक", app.ApplicationNumber); Row("अर्जदाराचे नाव", app.ApplicantName); Row("मंजूर सेवा / जागा प्रकार", app.ServiceType.ToString()); Row("सेवा तपशील", app.ServiceDescription); Row("लांबी", Measure(app.LengthFt, "ft")); Row("रुंदी", Measure(app.WidthFt, "ft")); Row("एकूण क्षेत्रफळ", Measure(app.AreaSqFt, "sq.ft."));
                Row("मंजूर / अंतिम देय रक्कम", $"₹{workflow.PayableAmount:N2}"); Row("प्रत्यक्ष भरलेली रक्कम", $"₹{workflow.PayableAmount:N2}"); Row("पेमेंट दिनांक", workflow.PaymentDate?.ToString("dd-MM-yyyy")); Row("UTR / पेमेंट संदर्भ", workflow.Utr);
                Row("परवानगी सुरू दिनांक", app.StartDate.ToString("dd-MM-yyyy")); Row("परवानगी समाप्ती दिनांक", app.EndDate.ToString("dd-MM-yyyy")); Row("एकूण दिवस / मंजूर कालावधी", app.RequiredDuration); Row("मंजुरी दिनांक", workflow.ApprovedAt?.ToString("dd-MM-yyyy")); Row("मंजूरी प्राधिकारी", "सहाय्यक आयुक्त"); Row("अर्जदाराचा पत्ता", app.PermanentAddress); Row("ठिकाण / प्रभाग", $"{app.Location} / {app.Prabhag}");
                Section("नियम व अटी");
                var terms = new[] { "1. एका अर्जदारास एकाच जागेसाठी फक्त एकदाच अर्ज करता येईल.", "2. परवाना मंजूर करणे, नाकारणे किंवा रद्द करणे याबाबतचा संपूर्ण अधिकार सोलापूर महानगरपालिकेकडे राहील.", "3. अर्जामध्ये दिलेली माहिती किंवा कागदपत्रे चुकीची, अपूर्ण किंवा बनावट आढळल्यास अर्ज/परवाना कोणतीही पूर्वसूचना न देता रद्द करण्यात येईल.", "4. महानगरपालिकेने वेळोवेळी लागू केलेले शुल्क, नियम, अटी व प्रशासकीय निर्णय अर्जदारास बंधनकारक राहतील.", "5. अर्जदाराने मंजूर झालेली जागा स्वतःच्या वापरासाठीच वापरणे बंधनकारक राहील. जागेचा गैरवापर, अनधिकृत हस्तांतरण किंवा इतर कोणताही गैरप्रकार आढळल्यास संबंधित परवाना रद्द करण्याचा अधिकार सोलापूर महानगरपालिकेकडे राहील." };
                foreach (var term in terms) c.Item().PaddingTop(3).Text(term).FontSize(8);
            });
            page.Footer().AlignCenter().Text($"परवाना तयार दिनांक: {workflow.CertificateGeneratedAt:dd-MM-yyyy}");
        })).GeneratePdf();
    }


}

