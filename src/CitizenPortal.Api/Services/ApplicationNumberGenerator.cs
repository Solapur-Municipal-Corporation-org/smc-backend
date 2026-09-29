namespace CitizenPortal.Api.Services;

public class ApplicationNumberGenerator : IApplicationNumberGenerator
{
    public string CurrentFinancialYear()
    {
        var now = DateTime.UtcNow;
        var startYear = now.Month >= 4 ? now.Year : now.Year - 1;
        return $"{startYear}-{(startYear + 1).ToString().Substring(2)}";
    }

    public string Generate(string departmentCode)
    {
        var fy = CurrentFinancialYear().Replace("-", "");
        var random = new Random().Next(100000, 999999);
        return $"CP/{departmentCode}/{fy}/{random}";
    }
}
