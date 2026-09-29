using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>Birth &amp; Death Registration Department (BDR) online services.</summary>
public class BdrServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "BDR";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "Birth Certificate",
            Description = "Apply for a registered birth certificate.",
            Fee = 30,
            ProcessingDays = 5,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Hospital Discharge Summary" },
                new() { DocumentName = "Parents' Aadhaar Card" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Child's Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Date of Birth", FieldType = FieldType.Date, Required = true, DisplayOrder = 2 },
                new() { Label = "Place of Birth", FieldType = FieldType.Text, Required = true, DisplayOrder = 3 },
                new() { Label = "Father's Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 4 },
                new() { Label = "Mother's Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 5 },
            }
        },
        new Service
        {
            Name = "Death Certificate",
            Description = "Apply for a registered death certificate.",
            Fee = 30,
            ProcessingDays = 5,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Hospital Death Summary" },
                new() { DocumentName = "Aadhaar Card of Deceased" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Deceased's Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Date of Death", FieldType = FieldType.Date, Required = true, DisplayOrder = 2 },
                new() { Label = "Place of Death", FieldType = FieldType.Text, Required = true, DisplayOrder = 3 },
                new() { Label = "Informant's Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 4 },
            }
        },
    };
}
