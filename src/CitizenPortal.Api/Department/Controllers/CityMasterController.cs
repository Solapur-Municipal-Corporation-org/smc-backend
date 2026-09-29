using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>CityMaster (tbl_city) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class CityMasterController : GenericCrudControllerStringKey<CityMaster>
{
    public CityMasterController(IRepository<CityMaster> repo) : base(repo) { }
    protected override object GetId(CityMaster entity) => entity.CityCode;
}
