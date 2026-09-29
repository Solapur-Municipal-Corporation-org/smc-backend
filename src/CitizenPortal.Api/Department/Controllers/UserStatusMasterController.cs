using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>UserStatusMaster (tbl_ust) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class UserStatusMasterController : GenericCrudControllerStringKey<UserStatusMaster>
{
    public UserStatusMasterController(IRepository<UserStatusMaster> repo) : base(repo) { }
    protected override object GetId(UserStatusMaster entity) => entity.UserCode;
}
