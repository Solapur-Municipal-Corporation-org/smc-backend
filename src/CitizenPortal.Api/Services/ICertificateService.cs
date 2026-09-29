using CitizenPortal.Api.Models;

namespace CitizenPortal.Api.Services;

public interface ICertificateService
{
    byte[] GenerateCertificatePdf(Application application, Citizen citizen, string serviceName, string departmentName);
}
