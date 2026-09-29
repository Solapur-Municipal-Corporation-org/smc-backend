using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Education master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class EducationController : GenericCrudController<Education>
{
    public EducationController(IRepository<Education> repo) : base(repo) { }
    protected override object GetId(Education entity) => entity.EducationId;
}
