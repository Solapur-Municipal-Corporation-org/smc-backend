using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>General Administration Department (GAD) online services.</summary>
public class GadServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "GAD";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "Income Certificate",
            Description = "Apply for an official income certificate for the current financial year.",
            Fee = 50,
            ProcessingDays = 7,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Aadhaar Card" },
                new() { DocumentName = "Salary Slip / Income Proof" },
                new() { DocumentName = "Address Proof" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Applicant Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Father's / Guardian's Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 2 },
                new() { Label = "Annual Income (Rs.)", FieldType = FieldType.Number, Required = true, DisplayOrder = 3 },
                new() { Label = "Purpose of Certificate", FieldType = FieldType.Select, Required = true, DisplayOrder = 4, Options = "Education,Employment,Government Scheme,Other" },
                new() { Label = "Residential Address", FieldType = FieldType.Textarea, Required = true, DisplayOrder = 5 },
            }
        },
        new Service
        {
            Name = "Domicile Certificate",
            Description = "Proof of residency issued to permanent residents.",
            Fee = 100,
            ProcessingDays = 10,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Aadhaar Card" },
                new() { DocumentName = "Ration Card" },
                new() { DocumentName = "Residence Proof (10 yrs)" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Applicant Full Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Date of Birth", FieldType = FieldType.Date, Required = true, DisplayOrder = 2 },
                new() { Label = "Years of Residence", FieldType = FieldType.Number, Required = true, DisplayOrder = 3 },
                new() { Label = "Residential Address", FieldType = FieldType.Textarea, Required = true, DisplayOrder = 4 },
            }
        },
    };
}
