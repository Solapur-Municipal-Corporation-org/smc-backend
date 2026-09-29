using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>CasteMaster (tbl_cast) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class CasteMasterController : GenericCrudControllerStringKey<CasteMaster>
{
    public CasteMasterController(IRepository<CasteMaster> repo) : base(repo) { }
    protected override object GetId(CasteMaster entity) => entity.CastCode;
}
