using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>
/// File upload endpoint used by the Employee "Documents" step. A file is uploaded here
/// first (independent of any employee record — works for brand-new employees too, since
/// they don't have an EmployeeId yet), and the returned <c>filePath</c> is then stored on
/// the EmployeeDocument row (FilePath/FileName/ContentType) when the wizard is saved.
/// Files are served back statically from wwwroot, so filePath can be used directly as an
/// &lt;img src&gt; or download link once the API base path is prefixed on the frontend.
/// </summary>
[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf" };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public UploadsController(IWebHostEnvironment env) => _env = env;

    /// <summary>Uploads one document/image. Returns the URL to store as EmployeeDocument.FilePath.</summary>
    [HttpPost("employee-document")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> UploadEmployeeDocument(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file was provided." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "File is larger than 10 MB." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { message = "Only JPG, PNG, GIF, WEBP and PDF files are allowed." });

        var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var subFolder = Path.Combine("uploads", "employee-documents", DateTime.UtcNow.ToString("yyyy-MM"));
        var folderPath = Path.Combine(webRoot, subFolder);
        Directory.CreateDirectory(folderPath);

        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folderPath, safeFileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = "/" + Path.Combine(subFolder, safeFileName).Replace('\\', '/');

        return Ok(new
        {
            filePath = relativeUrl,
            fileName = file.FileName,
            contentType = string.IsNullOrEmpty(file.ContentType) ? GuessContentType(ext) : file.ContentType,
            sizeBytes = file.Length
        });
    }

    /// <summary>Best-effort cleanup: deletes a previously uploaded file when its document row is removed.</summary>
    [HttpDelete("employee-document")]
    public IActionResult DeleteEmployeeDocument([FromQuery] string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return BadRequest(new { message = "path is required." });

        var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var normalized = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, normalized));
        var uploadsRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads", "employee-documents"));

        // Guard against path traversal — only ever delete inside the employee-documents folder.
        if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Invalid path." });

        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        return NoContent();
    }

    private static string GuessContentType(string ext) => ext switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        _ => "application/octet-stream"
    };
}
