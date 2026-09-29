namespace CitizenPortal.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string subFolder);
}
