using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Holiday master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class HolidayController : GenericCrudController<Holiday>
{
    public HolidayController(IRepository<Holiday> repo) : base(repo) { }
    protected override object GetId(Holiday entity) => entity.HolidayId;
}
