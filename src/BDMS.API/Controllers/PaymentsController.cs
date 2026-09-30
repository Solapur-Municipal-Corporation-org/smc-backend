using BDMS.Application.Services;
using BDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BDMS.API.Controllers;

public record InitiatePaymentRequest(string ApplicationNumber);
public record ConfirmPaymentRequest(string TransactionId);

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly BdmsDbContext _db;
    private readonly IPaymentService _payments;

    public PaymentsController(BdmsDbContext db, IPaymentService payments)
    {
        _db = db;
        _payments = payments;
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentRequest req)
    {
        var app = await _db.BirthApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == req.ApplicationNumber);
        if (app == null) return NotFound(new { message = "Application not found." });

        var record = await _payments.InitiateAsync(app.ApplicationNumber, app.AmountToPay);
        // In legacy this redirected to a real gateway URL. Here we just return the dummy
        // transaction id — the citizen portal shows a mock "Pay Now" screen and calls /confirm.
        return Ok(new { record.TransactionId, record.Amount, record.TransactionStatus });
    }

    // Maps the effect of legacy Responce.aspx callback (dummy — no real gateway involved)
    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] ConfirmPaymentRequest req)
    {
        var record = await _payments.ConfirmAsync(req.TransactionId);
        return Ok(new { record.TransactionId, record.TransactionStatus, record.Amount });
    }
}
