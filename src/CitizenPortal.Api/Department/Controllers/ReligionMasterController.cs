using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>ReligionMaster (tbl_rel) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class ReligionMasterController : GenericCrudControllerStringKey<ReligionMaster>
{
    public ReligionMasterController(IRepository<ReligionMaster> repo) : base(repo) { }
    protected override object GetId(ReligionMaster entity) => entity.ReligenCode;
}
