using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/transaction")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    public TransactionController(ITransactionService transactionService) => _transactionService = transactionService;

    [HttpGet("application/{applicationId:guid}")]
    public async Task<IActionResult> GetByApplication(Guid applicationId) =>
        Ok(await _transactionService.GetByApplicationAsync(applicationId));
}
