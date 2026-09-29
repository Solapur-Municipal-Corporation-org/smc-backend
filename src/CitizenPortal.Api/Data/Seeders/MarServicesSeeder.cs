using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>Marriage Registration Department (MAR) online services.</summary>
public class MarServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "MAR";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "Marriage Certificate",
            Description = "Apply for a registered marriage certificate.",
            Fee = 100,
            ProcessingDays = 10,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Aadhaar Card (Both Parties)" },
                new() { DocumentName = "Marriage Invitation Card / Proof of Marriage" },
                new() { DocumentName = "Passport Size Photographs (Both Parties)" },
                new() { DocumentName = "Witness Aadhaar Cards (2 Witnesses)" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Groom's Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Bride's Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 2 },
                new() { Label = "Date of Marriage", FieldType = FieldType.Date, Required = true, DisplayOrder = 3 },
                new() { Label = "Place of Marriage", FieldType = FieldType.Text, Required = true, DisplayOrder = 4 },
                new() { Label = "Witness 1 Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 5 },
                new() { Label = "Witness 2 Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 6 },
            }
        },
    };
}
