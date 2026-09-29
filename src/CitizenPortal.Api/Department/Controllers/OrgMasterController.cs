using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>OrgMaster (tbl_org) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class OrgMasterController : GenericCrudControllerStringKey<OrgMaster>
{
    public OrgMasterController(IRepository<OrgMaster> repo) : base(repo) { }
    protected override object GetId(OrgMaster entity) => entity.OrgCode;
}
