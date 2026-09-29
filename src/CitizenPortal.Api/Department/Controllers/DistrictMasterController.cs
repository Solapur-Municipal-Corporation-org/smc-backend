using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>DistrictMaster (tbl_dst) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class DistrictMasterController : GenericCrudControllerStringKey<DistrictMaster>
{
    public DistrictMasterController(IRepository<DistrictMaster> repo) : base(repo) { }
    protected override object GetId(DistrictMaster entity) => entity.DestCode;
}
