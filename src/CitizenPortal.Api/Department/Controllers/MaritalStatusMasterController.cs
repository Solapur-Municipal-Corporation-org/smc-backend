using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>MaritalStatusMaster (tbl_mrs) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class MaritalStatusMasterController : GenericCrudControllerStringKey<MaritalStatusMaster>
{
    public MaritalStatusMasterController(IRepository<MaritalStatusMaster> repo) : base(repo) { }
    protected override object GetId(MaritalStatusMaster entity) => entity.MarrStaCode;
}
