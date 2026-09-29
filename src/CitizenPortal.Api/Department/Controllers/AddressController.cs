using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Address master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class AddressController : GenericCrudController<Address>
{
    public AddressController(IRepository<Address> repo) : base(repo) { }
    protected override object GetId(Address entity) => entity.AddressId;
}
