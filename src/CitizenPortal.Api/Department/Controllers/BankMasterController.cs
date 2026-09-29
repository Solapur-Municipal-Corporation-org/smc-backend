using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>BankMaster (tbl_bank) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class BankMasterController : GenericCrudControllerStringKey<BankMaster>
{
    public BankMasterController(IRepository<BankMaster> repo) : base(repo) { }
    protected override object GetId(BankMaster entity) => entity.BankCode;
}
