namespace CitizenPortal.Api.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder)
    {
        var uploadsRoot = Path.Combine(_env.ContentRootPath, "Uploads", subFolder);
        Directory.CreateDirectory(uploadsRoot);

        var safeName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadsRoot, safeName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Path.Combine("Uploads", subFolder, safeName);
    }
}
