using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>City master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class CityController : GenericCrudController<City>
{
    public CityController(IRepository<City> repo) : base(repo) { }
    protected override object GetId(City entity) => entity.CityId;
}
