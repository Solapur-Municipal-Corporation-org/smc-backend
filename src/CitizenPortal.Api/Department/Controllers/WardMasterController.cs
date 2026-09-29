using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>WardMaster (tbl_ward) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class WardMasterController : GenericCrudControllerStringKey<WardMaster>
{
    public WardMasterController(IRepository<WardMaster> repo) : base(repo) { }
    protected override object GetId(WardMaster entity) => entity.WardCode;
}
