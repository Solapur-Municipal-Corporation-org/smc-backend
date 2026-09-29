using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>BloodGroupMaster (tbl_bloodgrp) — CRUD via the shared string-keyed GenericCrudController.</summary>
[Route("api/masters/[controller]")]
public class BloodGroupMasterController : GenericCrudControllerStringKey<BloodGroupMaster>
{
    public BloodGroupMasterController(IRepository<BloodGroupMaster> repo) : base(repo) { }
    protected override object GetId(BloodGroupMaster entity) => entity.BldGrpCode;
}
