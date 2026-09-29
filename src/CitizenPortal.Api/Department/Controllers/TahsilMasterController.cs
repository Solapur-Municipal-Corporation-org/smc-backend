using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>TahsilMaster (tbl_tahl) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class TahsilMasterController : GenericCrudControllerStringKey<TahsilMaster>
{
    public TahsilMasterController(IRepository<TahsilMaster> repo) : base(repo) { }
    protected override object GetId(TahsilMaster entity) => entity.TahashilCode;
}
