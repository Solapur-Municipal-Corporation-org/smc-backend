using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>ZoneMaster (tbl_zone) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class ZoneMasterController : GenericCrudControllerStringKey<ZoneMaster>
{
    public ZoneMasterController(IRepository<ZoneMaster> repo) : base(repo) { }
    protected override object GetId(ZoneMaster entity) => entity.ZoneCode;
}
