using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>OccupationMaster (tbl_occu) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class OccupationMasterController : GenericCrudControllerStringKey<OccupationMaster>
{
    public OccupationMasterController(IRepository<OccupationMaster> repo) : base(repo) { }
    protected override object GetId(OccupationMaster entity) => entity.OccupCode;
}
