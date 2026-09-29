using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>Garden & Parks Department (GPD) online services.</summary>
public class GpdServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "GPD";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "Tree Cutting",
            Description = "Apply for permission to cut trees and track the application from draft to approval.",
            Fee = 0,
            ProcessingDays = 15,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Aadhaar Card" },
                new() { DocumentName = "Property Tax Receipt" },
                new() { DocumentName = "Site Sketch" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Application Type", FieldType = FieldType.Select, Required = true, DisplayOrder = 1, Options = "Fresh Application,Renewal,Emergency" },
                new() { Label = "Applicant Type", FieldType = FieldType.Select, Required = true, DisplayOrder = 2, Options = "Owner,Occupier,Authorized Representative" },
                new() { Label = "Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 3 },
                new() { Label = "Address", FieldType = FieldType.Textarea, Required = true, DisplayOrder = 4 },
                new() { Label = "Email ID", FieldType = FieldType.Email, Required = true, DisplayOrder = 5 },
                new() { Label = "Mobile Number", FieldType = FieldType.Tel, Required = true, DisplayOrder = 6 },
                new() { Label = "Aadhar Number", FieldType = FieldType.Text, Required = true, DisplayOrder = 7 },
                new() { Label = "Peth Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 8 },
                new() { Label = "Peth Number", FieldType = FieldType.Number, Required = true, DisplayOrder = 9 },
                new() { Label = "Zone Number", FieldType = FieldType.Number, Required = true, DisplayOrder = 10 },
                new() { Label = "Prabhag Number", FieldType = FieldType.Number, Required = true, DisplayOrder = 11 },
                new() { Label = "Property Tax Number", FieldType = FieldType.Text, Required = true, DisplayOrder = 12 },
                new() { Label = "Tree Address", FieldType = FieldType.Textarea, Required = true, DisplayOrder = 13 },
                new() { Label = "Tree Cutting Reason", FieldType = FieldType.Textarea, Required = true, DisplayOrder = 14 },
                new() { Label = "Number of Trees", FieldType = FieldType.Number, Required = true, DisplayOrder = 15 },
                new() { Label = "Tree Species", FieldType = FieldType.Text, Required = true, DisplayOrder = 16 },
            }
        }
    };
}
