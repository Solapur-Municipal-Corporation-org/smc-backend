using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>State master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class StateController : GenericCrudController<State>
{
    public StateController(IRepository<State> repo) : base(repo) { }
    protected override object GetId(State entity) => entity.StateId;
}
