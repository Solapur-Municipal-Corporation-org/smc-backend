using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Family master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class FamilyController : GenericCrudController<Family>
{
    public FamilyController(IRepository<Family> repo) : base(repo) { }
    protected override object GetId(Family entity) => entity.FamilyId;
}
