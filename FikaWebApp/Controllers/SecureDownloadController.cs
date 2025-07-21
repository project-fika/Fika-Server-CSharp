using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers
{
    [Route("api/secure-download")]
    [ApiController]
    public class SecureDownloadController : ControllerBase
    {
        [HttpGet("{filename}")]
        [Authorize]
        public IActionResult GetFile(string filename)
        {
            var filePath = Path.Combine("ProtectedFiles", filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = "application/octet-stream";
            return File(fileBytes, contentType, filename);
        }
    }
}
