using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>LocationMaster (tbl_location) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class LocationMasterController : GenericCrudControllerStringKey<LocationMaster>
{
    public LocationMasterController(IRepository<LocationMaster> repo) : base(repo) { }
    protected override object GetId(LocationMaster entity) => entity.LocCode;
}
