using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Route("api/secure-download")]
[ApiController]
public class SecureDownloadController : ControllerBase
{
    [HttpGet("{*filename}")]
    [Authorize]
    public IActionResult GetFile(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
        {
            return BadRequest("Filename is required.");
        }

        // Root directory where secure files are stored
        var rootPath = Path.GetFullPath("ProtectedFiles");

        // Combine root path with requested filename
        var fullPath = Path.GetFullPath(Path.Combine(rootPath, filename));

        // Prevent access outside of ProtectedFiles
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Invalid path");
        }

        if (!System.IO.File.Exists(fullPath))
        {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(fullPath);
        var contentType = "application/octet-stream";

        // Only return the actual filename, not the full relative path
        return File(fileBytes, contentType, Path.GetFileName(fullPath));
    }
}
