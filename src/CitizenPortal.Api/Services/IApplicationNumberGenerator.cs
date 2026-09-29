namespace CitizenPortal.Api.Services;

public interface IApplicationNumberGenerator
{
    string Generate(string departmentCode);
    string CurrentFinancialYear();
}
