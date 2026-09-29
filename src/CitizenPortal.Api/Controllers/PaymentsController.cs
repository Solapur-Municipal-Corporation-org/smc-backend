using System.Security.Claims;
using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using CitizenPortal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentsController(AppDbContext db)
    {
        _db = db;
    }

    private Guid CurrentCitizenId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("initiate")]
    public async Task<ActionResult<PaymentReceiptResponse>> Initiate([FromBody] PaymentInitiateRequest request)
    {
        var application = await _db.Applications
            .Include(a => a.Service)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && a.CitizenId == CurrentCitizenId);

        if (application is null) return NotFound(new { message = "Application not found." });
        if (application.Service!.Fee <= 0) return BadRequest(new { message = "This service does not require a payment." });

        if (application.Payment is null)
        {
            application.Payment = new Payment
            {
                ApplicationId = application.Id,
                ReceiptNumber = $"RCPT/{DateTime.UtcNow:yyyyMMdd}/{new Random().Next(100000, 999999)}",
                Amount = application.Service.Fee,
                Status = PaymentStatus.Initiated,
            };
            await _db.SaveChangesAsync();
        }

        return Ok(new { paymentId = application.Payment.Id, receiptNumber = application.Payment.ReceiptNumber, amount = application.Payment.Amount });
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<PaymentReceiptResponse>> Confirm([FromBody] PaymentConfirmRequest request)
    {
        var application = await _db.Applications
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && a.CitizenId == CurrentCitizenId);

        if (application?.Payment is null) return NotFound(new { message = "No pending payment for this application." });

        application.Payment.Status = PaymentStatus.Success;
        application.Payment.TransactionId = request.TransactionId;
        application.Payment.PaymentMode = request.PaymentMode;
        application.Payment.PaidOn = DateTime.UtcNow;

        if (application.Status == ApplicationStatus.Pending)
            application.Status = ApplicationStatus.UnderReview;
        application.UpdatedOn = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(MapReceipt(application));
    }

    [HttpGet("receipt/{applicationId:guid}")]
    public async Task<ActionResult<PaymentReceiptResponse>> GetReceipt(Guid applicationId)
    {
        var application = await _db.Applications
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.CitizenId == CurrentCitizenId);

        if (application?.Payment is null || application.Payment.Status != PaymentStatus.Success)
            return NotFound(new { message = "No successful payment found for this application." });

        return Ok(MapReceipt(application));
    }

    private static PaymentReceiptResponse MapReceipt(Application a) => new()
    {
        ReceiptNumber = a.Payment!.ReceiptNumber,
        ApplicationNumber = a.ApplicationNumber,
        Amount = a.Payment.Amount,
        PaidOn = a.Payment.PaidOn,
        PaymentMode = a.Payment.PaymentMode,
        TransactionId = a.Payment.TransactionId,
    };
}
