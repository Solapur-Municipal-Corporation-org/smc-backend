using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>EmployeeDocument master — CRUD via the shared GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class EmployeeDocumentController : GenericCrudController<EmployeeDocument>
{
    public EmployeeDocumentController(IRepository<EmployeeDocument> repo) : base(repo) { }
    protected override object GetId(EmployeeDocument entity) => entity.DocumentId;
}
