using BDMS.Application.Services;
using BDMS.Domain.Models;
using BDMS.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Dummy payment gateway. Legacy flow: browser redirected to a real gateway, gateway posted
/// back to Responce.aspx which updated Payment_Made_Yes_No / Transaction_Status on the
/// application. Here, "InitiateAsync" creates a fake session and "ConfirmAsync" (triggered by
/// a Pay Now click in the citizen portal, no real money moves) does the same DB update the
/// legacy callback did. Swap for Razorpay/PayU etc. later behind this same interface.
/// </summary>
public class DummyPaymentService : IPaymentService
{
    private readonly BdmsDbContext _db;
    private readonly IConfiguration _config;

    public DummyPaymentService(BdmsDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<PaymentRecord> InitiateAsync(string applicationNumber, decimal amount)
    {
        var record = new PaymentRecord
        {
            ApplicationNumber = applicationNumber,
            Amount = amount,
            TransactionId = $"DUMMY-{Guid.NewGuid():N}".Substring(0, 20),
            TransactionStatus = "Initiated",
            IsDummy = true
        };
        _db.PaymentRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<PaymentRecord> ConfirmAsync(string transactionId)
    {
        var record = await _db.PaymentRecords.FirstOrDefaultAsync(p => p.TransactionId == transactionId)
            ?? throw new InvalidOperationException("Unknown transaction.");

        var alwaysSucceeds = _config.GetValue<bool>("DummyServices:PaymentAlwaysSucceeds");
        record.TransactionStatus = alwaysSucceeds ? "Success" : "Failed";

        if (record.TransactionStatus == "Success")
        {
            var app = await _db.BirthApplications
                .FirstOrDefaultAsync(a => a.ApplicationNumber == record.ApplicationNumber);
            if (app != null)
            {
                app.PaymentMadeYesNo = PaymentStatus.Yes;
                app.PaidAmount = record.Amount;
                app.AckId = $"ACK_{app.ApplicationNumber}";
                app.AckSubject = $"Birth Registration Acknowledgement - {app.ChildNameEnglish}";
            }
        }

        await _db.SaveChangesAsync();
        return record;
    }
}
