using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>RequiredDocumentMaster (tbl_dc) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class RequiredDocumentMasterController : GenericCrudControllerStringKey<RequiredDocumentMaster>
{
    public RequiredDocumentMasterController(IRepository<RequiredDocumentMaster> repo) : base(repo) { }
    protected override object GetId(RequiredDocumentMaster entity) => entity.DocCode;
}
