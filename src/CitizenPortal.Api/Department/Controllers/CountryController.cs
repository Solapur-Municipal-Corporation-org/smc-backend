using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Country master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class CountryController : GenericCrudController<Country>
{
    public CountryController(IRepository<Country> repo) : base(repo) { }
    protected override object GetId(Country entity) => entity.CountryId;
}
