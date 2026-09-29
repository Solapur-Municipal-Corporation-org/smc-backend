using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>District master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class DistrictController : GenericCrudController<District>
{
    public DistrictController(IRepository<District> repo) : base(repo) { }
    protected override object GetId(District entity) => entity.DistrictId;
}
