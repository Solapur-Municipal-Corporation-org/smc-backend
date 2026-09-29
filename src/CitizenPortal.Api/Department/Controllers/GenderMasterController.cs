using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>GenderMaster (tbl_gender) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class GenderMasterController : GenericCrudControllerStringKey<GenderMaster>
{
    public GenderMasterController(IRepository<GenderMaster> repo) : base(repo) { }
    protected override object GetId(GenderMaster entity) => entity.TitleCode;
}
