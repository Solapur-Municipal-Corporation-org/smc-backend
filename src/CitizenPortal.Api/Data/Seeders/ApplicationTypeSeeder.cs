using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Data.Seeders;

public static class ApplicationTypeSeeder
{
    public static async Task SeedApplicationTypesAsync(AppDbContext db)
    {
        // Only seed if no application types exist
        if (await db.ApplicationTypes.AnyAsync())
            return;

        // Get services for different departments
        var incomeCertService = await db.Services.FirstOrDefaultAsync(s => s.Name == "Income Certificate");
        var domicileService = await db.Services.FirstOrDefaultAsync(s => s.Name == "Domicile Certificate");
        var birthCertService = await db.Services.FirstOrDefaultAsync(s => s.Name == "Birth Certificate");
        var deathCertService = await db.Services.FirstOrDefaultAsync(s => s.Name == "Death Certificate");
        var waterConnService = await db.Services.FirstOrDefaultAsync(s => s.Name == "New Water Connection");

        // Seed only for services that exist
        var typesToAdd = new List<ApplicationType>();

        // Income Certificate Application Types
        if (incomeCertService is not null)
        {
            typesToAdd.Add(new ApplicationType
            {
                ServiceId = incomeCertService.Id,
                Name = "Fresh",
                Description = "New income certificate application",
                DisplayOrder = 1,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Individual", DisplayOrder = 1 },
                    new() { Name = "Family Head", DisplayOrder = 2 },
                    new() { Name = "Self Employed", DisplayOrder = 3 },
                }
            });

            typesToAdd.Add(new ApplicationType
            {
                ServiceId = incomeCertService.Id,
                Name = "Duplicate",
                Description = "Duplicate copy of existing certificate",
                DisplayOrder = 2,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Original Applicant", DisplayOrder = 1 },
                    new() { Name = "Authorized Representative", DisplayOrder = 2 },
                }
            });
        }

        // Domicile Certificate Application Types
        if (domicileService is not null)
        {
            typesToAdd.Add(new ApplicationType
            {
                ServiceId = domicileService.Id,
                Name = "Fresh",
                DisplayOrder = 1,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Permanent Resident", DisplayOrder = 1 },
                    new() { Name = "Temporary Resident", DisplayOrder = 2 },
                }
            });
        }

        // Death Certificate Application Types
        if (birthCertService is not null)
        {
            typesToAdd.Add(new ApplicationType
            {
                ServiceId = birthCertService.Id,
                Name = "Birth Certificate",
                DisplayOrder = 1,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Parents", DisplayOrder = 1 },
                    new() { Name = "Applicant (18+)", DisplayOrder = 2 },
                    new() { Name = "Authorized Person", DisplayOrder = 3 },
                }
            });

        }

        if (deathCertService is not null)
        {
            typesToAdd.Add(new ApplicationType
            {
                ServiceId = deathCertService.Id,
                Name = "Death Certificate",
                DisplayOrder = 2,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Family Member", DisplayOrder = 1 },
                    new() { Name = "Authorized Representative", DisplayOrder = 2 },
                }
            });
        }

        // Water Connection Application Types
        if (waterConnService is not null)
        {
            typesToAdd.Add(new ApplicationType
            {
                ServiceId = waterConnService.Id,
                Name = "Residential",
                DisplayOrder = 1,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Owner", DisplayOrder = 1 },
                    new() { Name = "Tenant", DisplayOrder = 2 },
                }
            });

            typesToAdd.Add(new ApplicationType
            {
                ServiceId = waterConnService.Id,
                Name = "Commercial",
                DisplayOrder = 2,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Business Owner", DisplayOrder = 1 },
                    new() { Name = "Authorized Signatory", DisplayOrder = 2 },
                }
            });

            typesToAdd.Add(new ApplicationType
            {
                ServiceId = waterConnService.Id,
                Name = "Industrial",
                DisplayOrder = 3,
                ApplicantTypes = new List<ApplicantType>
                {
                    new() { Name = "Plant Manager", DisplayOrder = 1 },
                    new() { Name = "Authorized Representative", DisplayOrder = 2 },
                }
            });
        }

        if (typesToAdd.Any())
        {
            await db.ApplicationTypes.AddRangeAsync(typesToAdd);
            await db.SaveChangesAsync();
        }
    }
}
