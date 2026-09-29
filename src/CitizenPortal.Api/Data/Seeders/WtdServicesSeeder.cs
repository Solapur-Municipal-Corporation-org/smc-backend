using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>
/// Water Tax Department (WTD) online services. This is a real, working example of
/// the "no custom table needed" integration path — see Modules/WTD for the
/// companion example of the "custom table needed" path.
/// </summary>
public class WtdServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "WTD";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "New Water Tax Connection Assessment",
            Description = "Request an on-site inspection to assess a new property for water tax registration.",
            Fee = 150,
            ProcessingDays = 14,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Property Ownership Proof" },
                new() { DocumentName = "Water Connection Proof" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Applicant Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Property ID / PT Number", FieldType = FieldType.Text, Required = true, DisplayOrder = 2 },
                new() { Label = "Property Type", FieldType = FieldType.Select, Required = true, DisplayOrder = 3, Options = "Residential,Commercial,Mixed" },
                new() { Label = "Contact Mobile Number", FieldType = FieldType.Tel, Required = true, DisplayOrder = 4 },
            }
        },
    };
}
