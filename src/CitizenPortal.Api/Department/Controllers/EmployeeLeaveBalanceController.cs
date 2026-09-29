using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>EmployeeLeaveBalance master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class EmployeeLeaveBalanceController : GenericCrudController<EmployeeLeaveBalance>
{
    public EmployeeLeaveBalanceController(IRepository<EmployeeLeaveBalance> repo) : base(repo) { }
    protected override object GetId(EmployeeLeaveBalance entity) => entity.LeaveBalanceId;
}
