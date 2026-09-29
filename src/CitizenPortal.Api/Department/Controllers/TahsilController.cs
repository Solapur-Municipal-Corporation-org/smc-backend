using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Tahsil master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class TahsilController : GenericCrudController<Tahsil>
{
    public TahsilController(IRepository<Tahsil> repo) : base(repo) { }
    protected override object GetId(Tahsil entity) => entity.TahsilId;
}
