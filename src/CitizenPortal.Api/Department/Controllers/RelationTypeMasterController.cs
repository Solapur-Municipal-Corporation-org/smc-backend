using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>RelationTypeMaster (tbl_relt) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class RelationTypeMasterController : GenericCrudControllerStringKey<RelationTypeMaster>
{
    public RelationTypeMasterController(IRepository<RelationTypeMaster> repo) : base(repo) { }
    protected override object GetId(RelationTypeMaster entity) => entity.RelCode;
}
