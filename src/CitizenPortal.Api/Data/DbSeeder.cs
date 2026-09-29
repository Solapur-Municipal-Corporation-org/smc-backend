using CitizenPortal.Api.Data.Seeders;
using CitizenPortal.Api.Models;
using CitizenPortal.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var dbProvider = db.Database.ProviderName;

        // The live smc_db schema is a legacy application database and does not match
        // the generic EF model used by the portal. If the database already contains the
        // live MR_DEPT_* tables, skip the app's bootstrap seeding logic instead of
        // querying missing fields like Code/Name/Id on the legacy schema.
        if (dbProvider?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
        {
            var legacyDepartmentTableExists = await db.Database.SqlQueryRaw<int>("SELECT CASE WHEN OBJECT_ID('dbo.MR_DEPT_Departments', 'U') IS NOT NULL THEN 1 ELSE 0 END AS Value").SingleAsync();
            if (legacyDepartmentTableExists == 1)
            {
                return;
            }
        }

        // For in-memory SQLite, use EnsureCreatedAsync. For SQL Server, use MigrateAsync.
        if (dbProvider?.Contains("Sqlite") == true)
        {
            await db.Database.EnsureCreatedAsync();
            await EnsureSqliteTypeTablesAsync(db);
        }
        else
        {
            await db.Database.MigrateAsync();
        }

        // ---- 1) Master department registry (runs once, on an empty Departments table) ----
        // To add a brand-new department (not already in this list), just add a row here.
        if (!await db.Departments.AnyAsync())
        {
            // Official Corporation Department master list, reordered so Birth & Death
            // Registration and Marriage Registration appear first. Code, Name, IconName
            // (matches the lucide-react icon used on the frontend).
            var departmentDefs = new (string Code, string Name, string IconName)[]
            {
                ("BDR", "Birth & Death Registration Department", "FileText"),
                ("MAR", "Marriage Registration Department", "Heart"),
                ("GAD", "General Administration Department", "Landmark"),
                ("MCO", "Municipal Commissioner Office", "Briefcase"),
                ("ACO", "Additional Commissioner Office", "UserCog"),
                ("CED", "City Engineer Department", "HardHat"),
                ("PWD", "Public Works Department (PWD)", "Construction"),
                ("WS", "Water Supply Department", "Droplets"),
                ("SEW", "Sewerage Department", "Waves"),
                ("SWD", "Storm Water Drain Department", "CloudRain"),
                ("SWM", "Solid Waste Management (SWM)", "Trash2"),
                ("HLT", "Health Department", "HeartPulse"),
                ("MED", "Medical Department", "Stethoscope"),
                ("FES", "Fire & Emergency Services", "Flame"),
                ("TPD", "Town Planning Department", "Map"),
                ("BPD", "Building Permission Department", "Building2"),
                ("EST", "Estate Department", "Warehouse"),
                ("GPD", "Garden & Parks Department", "Trees"),
                ("ELE", "Electrical Department", "Zap"),
                ("MEC", "Mechanical Department", "Wrench"),
                ("AFD", "Accounts & Finance Department", "Calculator"),
                ("TRE", "Treasury Department", "Banknote"),
                ("AUD", "Audit Department", "ClipboardCheck"),
                ("PROP", "Property Tax Department", "Receipt"),
                ("WTD", "Water Tax Department", "Droplet"),
                ("LIC", "License Department", "BadgeCheck"),
                ("MKT", "Market Department", "Store"),
                ("ENC", "Encroachment Removal Department", "ShieldAlert"),
                ("LEG", "Legal Department", "Scale"),
                ("ITD", "Information Technology (IT) Department", "Cpu"),
                ("HRD", "Human Resource (HR) Department", "Users"),
                ("PAY", "Payroll Department", "Wallet"),
                ("SPD", "Stores & Purchase Department", "Package"),
                ("TND", "Tender Department", "FileSignature"),
                ("EDU", "Education Department", "GraduationCap"),
                ("SWF", "Social Welfare Department", "HeartHandshake"),
                ("WCW", "Women & Child Welfare Department", "Baby"),
                ("DMD", "Disaster Management Department", "AlertTriangle"),
                ("VET", "Veterinary Department", "PawPrint"),
                ("SID", "Slum Improvement Department", "Home"),
                ("SCU", "Smart City / Urban Development Department", "Building"),
                ("PRD", "Public Relations Department", "Megaphone"),
                ("CFW", "Citizen Facilitation / Ward Offices", "MessageSquare"),
            };

            var departments = departmentDefs.Select((d, index) => new Models.Department
            {
                Code = d.Code,
                Name = d.Name,
                IconName = d.IconName,
                DepartmentDescription = "Citizen",
                DisplayOrder = index,
            }).ToList();

            await db.Departments.AddRangeAsync(departments);
            await db.SaveChangesAsync();
        }

        // ---- 2) Service catalog (runs on every startup, additive & idempotent) ----
        // Every class implementing IServiceCatalogSeeder anywhere in this assembly
        // (Data/Seeders/*.cs for built-in departments, Modules/{Dept}/*Seeder.cs for
        // developer-contributed departments) is discovered automatically here. This
        // means a new developer only has to drop in a new seeder file — they never
        // need to touch this method, so there's nothing to merge-conflict over.
        await SeedServiceCatalogsAsync(db);
        
            // Application and applicant type options depend on the service catalog.
            await ApplicationTypeSeeder.SeedApplicationTypesAsync(db);

        await db.SaveChangesAsync();
    }

    private static async Task EnsureSqliteTypeTablesAsync(AppDbContext db)
    {
        // Older development files were created with EnsureCreated before these
        // tables were added. Create only the missing tables without deleting data.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ApplicationTypes" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ApplicationTypes" PRIMARY KEY,
                "ServiceId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Description" TEXT NULL,
                "DisplayOrder" INTEGER NOT NULL,
                CONSTRAINT "FK_ApplicationTypes_Services_ServiceId"
                    FOREIGN KEY ("ServiceId") REFERENCES "Services" ("Id") ON DELETE CASCADE
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ApplicantTypes" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_ApplicantTypes" PRIMARY KEY,
                "ApplicationTypeId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Description" TEXT NULL,
                "DisplayOrder" INTEGER NOT NULL,
                CONSTRAINT "FK_ApplicantTypes_ApplicationTypes_ApplicationTypeId"
                    FOREIGN KEY ("ApplicationTypeId") REFERENCES "ApplicationTypes" ("Id") ON DELETE CASCADE
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_ApplicationTypes_ServiceId"
            ON "ApplicationTypes" ("ServiceId");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_ApplicantTypes_ApplicationTypeId"
            ON "ApplicantTypes" ("ApplicationTypeId");
            """);
    }

    private static async Task SeedServiceCatalogsAsync(AppDbContext db)
    {
        var seeders = System.Reflection.Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IServiceCatalogSeeder).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .Select(t => (IServiceCatalogSeeder)Activator.CreateInstance(t)!)
            .ToList();

        foreach (var seeder in seeders)
        {
            var department = await db.Departments.FirstOrDefaultAsync(d => d.Code == seeder.DepartmentCode);
            if (department is null)
            {
                // Department code referenced by the seeder doesn't exist yet in the
                // master registry above — add it there first. We skip instead of
                // crashing so one misconfigured module doesn't take the whole app down.
                Console.WriteLine($"[DbSeeder] Skipped '{seeder.GetType().Name}': no Department with Code '{seeder.DepartmentCode}' found.");
                continue;
            }

            foreach (var service in seeder.GetServices())
            {
                var alreadyExists = await db.Services.AnyAsync(s => s.DepartmentId == department.Id && s.Name == service.Name);
                if (alreadyExists) continue;

                service.DepartmentId = department.Id;
                await db.Services.AddAsync(service);
            }
        }

        await db.SaveChangesAsync();
    }
}
