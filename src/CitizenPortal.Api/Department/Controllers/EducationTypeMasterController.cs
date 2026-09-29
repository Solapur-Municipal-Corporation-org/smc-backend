using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>EducationTypeMaster (tbl_edu) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class EducationTypeMasterController : GenericCrudControllerStringKey<EducationTypeMaster>
{
    public EducationTypeMasterController(IRepository<EducationTypeMaster> repo) : base(repo) { }
    protected override object GetId(EducationTypeMaster entity) => entity.EduCode;
}
