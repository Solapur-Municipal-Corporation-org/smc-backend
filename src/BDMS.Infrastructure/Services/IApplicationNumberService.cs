using BDMS.Application.Services;
using BDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BDMS.Infrastructure.Services;

/// <summary>
/// Exact port of legacy getTempCaseNumber() / getPerCaseNumber(): count existing rows,
/// add 1, zero-pad to 5 digits. Permanent numbers additionally get a "BCE" + fiscal-year-code
/// prefix, replicating the legacy hardcoded fiscal-year branching (kept as-is per your
/// "don't change any logic" instruction, including the fact it's a hardcoded date cutoff
/// rather than a computed fiscal year — that's how the legacy system does it).
/// </summary>
public class ApplicationNumberService : IApplicationNumberService
{
    private readonly BdmsDbContext _db;

    public ApplicationNumberService(BdmsDbContext db) => _db = db;

    public async Task<string> GenerateTempApplicationNumberAsync()
    {
        var count = await _db.TempBirthApplications.CountAsync();
        return PadTo5(count + 1);
    }

    public async Task<string> GeneratePermanentApplicationNumberAsync()
    {
        // Legacy: hardcoded cutoff dates decide the fiscal-year code prefix.
        var now = DateTime.Now;
        var cutoff2026 = new DateTime(2026, 3, 31);
        var cutoff2022 = new DateTime(2022, 3, 31);

        string fiscalCode;
        DateTime countSinceDate;

        if (now > cutoff2026)
        {
            fiscalCode = "2627";
            countSinceDate = cutoff2026;
        }
        else
        {
            fiscalCode = "2223";
            countSinceDate = cutoff2022;
        }

        var count = await _db.BirthApplications.CountAsync(a => a.EntryDate.Date > countSinceDate.Date);
        var padded = PadTo5(count + 1);

        return $"BCE{fiscalCode}{padded}";
    }

    public async Task<string> GenerateDeathTempApplicationNumberAsync()
        => PadTo5(await _db.TempDeathApplications.CountAsync() + 1);

    public async Task<string> GenerateDeathPermanentApplicationNumberAsync()
    {
        var now = DateTime.Now;
        var cutoff = new DateTime(2026, 3, 31);
        var fiscalCode = now > cutoff ? "2627" : "2223";
        var count = await _db.DeathApplications.CountAsync(a => a.EntryDate.Date > cutoff.Date);
        return $"DCE{fiscalCode}{PadTo5(count + 1)}";
    }

    private static string PadTo5(int number)
    {
        var s = number.ToString();
        return s.Length switch
        {
            1 => "0000" + s,
            2 => "000" + s,
            3 => "00" + s,
            4 => "0" + s,
            _ => s
        };
    }
}
