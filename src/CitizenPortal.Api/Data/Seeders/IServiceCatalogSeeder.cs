using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Data.Seeders;

/// <summary>
/// One implementation per department. DbSeeder discovers every implementation of
/// this interface automatically (via reflection) and attaches the returned
/// services to the matching Department row on first run.
///
/// To add a new department's services: create a new file in this folder named
/// "{DeptCode}ServicesSeeder.cs", implement this interface, and you're done —
/// no other file needs to change. This keeps multiple developers from editing
/// the same seeder file at once.
/// </summary>
public interface IServiceCatalogSeeder
{
    /// <summary>Must match a Department.Code already present in DbSeeder's department list.</summary>
    string DepartmentCode { get; }

    List<Service> GetServices();
}
