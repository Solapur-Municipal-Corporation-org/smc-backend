using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>StateMaster (tbl_state) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class StateMasterController : GenericCrudControllerStringKey<StateMaster>
{
    public StateMasterController(IRepository<StateMaster> repo) : base(repo) { }
    protected override object GetId(StateMaster entity) => entity.SatateCode;
}
