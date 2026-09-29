using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>FinancialYear master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class FinancialYearController : GenericCrudController<FinancialYear>
{
    public FinancialYearController(IRepository<FinancialYear> repo) : base(repo) { }
    protected override object GetId(FinancialYear entity) => entity.FinancialYearId;
}
