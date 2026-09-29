using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>TitleMaster (tbl_tit) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class TitleMasterController : GenericCrudControllerStringKey<TitleMaster>
{
    public TitleMasterController(IRepository<TitleMaster> repo) : base(repo) { }
    protected override object GetId(TitleMaster entity) => entity.TitleCode;
}
