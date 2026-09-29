using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>CountryMaster (tbl_ctr) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class CountryMasterController : GenericCrudControllerStringKey<CountryMaster>
{
    public CountryMasterController(IRepository<CountryMaster> repo) : base(repo) { }
    protected override object GetId(CountryMaster entity) => entity.ContryCode;
}
