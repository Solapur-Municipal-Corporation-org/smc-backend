using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>Property Tax Department (PROP) online services.</summary>
public class PropServicesSeeder : IServiceCatalogSeeder
{
    public string DepartmentCode => "PROP";

    public List<Service> GetServices() => new()
    {
        new Service
        {
            Name = "Property Tax Payment",
            Description = "Pay annual property tax for residential or commercial property.",
            Fee = 0,
            ProcessingDays = 1,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Property ID / PT Number" },
                new() { DocumentName = "Previous Tax Receipt" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Property ID (PT Number)", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "Owner Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 2 },
                new() { Label = "Property Type", FieldType = FieldType.Select, Required = true, DisplayOrder = 3, Options = "Residential,Commercial,Mixed" },
                new() { Label = "Assessed Value (Rs.)", FieldType = FieldType.Number, Required = true, DisplayOrder = 4 },
            }
        },
        new Service
        {
            Name = "Property Mutation",
            Description = "Transfer property ownership records after sale or inheritance.",
            Fee = 250,
            ProcessingDays = 21,
            RequiredDocuments = new List<ServiceDocument>
            {
                new() { DocumentName = "Sale Deed" },
                new() { DocumentName = "Aadhaar Card" },
                new() { DocumentName = "NOC" },
                new() { DocumentName = "Latest Tax Receipt" },
            },
            Fields = new List<ServiceField>
            {
                new() { Label = "Property ID", FieldType = FieldType.Text, Required = true, DisplayOrder = 1 },
                new() { Label = "New Owner Name", FieldType = FieldType.Text, Required = true, DisplayOrder = 2 },
                new() { Label = "Reason for Transfer", FieldType = FieldType.Select, Required = true, DisplayOrder = 3, Options = "Sale,Inheritance,Gift,Court Order" },
            }
        },
    };
}
