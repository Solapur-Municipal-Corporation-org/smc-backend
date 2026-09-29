using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>ClassMaster (tbl_class) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class ClassMasterController : GenericCrudControllerStringKey<ClassMaster>
{
    public ClassMasterController(IRepository<ClassMaster> repo) : base(repo) { }
    protected override object GetId(ClassMaster entity) => entity.ClassCode;
}
