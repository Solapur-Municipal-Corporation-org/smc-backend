using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>Water Supply Department (WS) online services.</summary>
public class WsServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "WS";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "New Water Connection",
            Description = "Apply for a new residential or commercial water connection.",
            Fee = 500,
            ProcessingDays = 15,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Property Ownership Proof" },
                new() { DocumentName = "Aadhaar Card" },
                new() { DocumentName = "Site Plan" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Applicant Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Connection Type", FieldType = FieldType.Select, Required = true, DisplayOrder = 2, Options = "Residential,Commercial,Industrial" },
                new() { Label = "Pipe Size (mm)", FieldType = FieldType.Select, Required = true, DisplayOrder = 3, Options = "15,20,25,40" },
                new() { Label = "Installation Address", FieldType = FieldType.Textarea, Required = true, DisplayOrder = 4 },
            }
        },
    };
}
