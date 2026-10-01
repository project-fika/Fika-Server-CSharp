using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public sealed class FileManagerController(
    IConfiguration configuration,
    ILogger<FileManagerController> logger) : ControllerBase
{
    public sealed class FileTreeNode
    {
        public required string Value { get; set; }
        public required string Text { get; set; }
        public bool IsDirectory { get; set; }
        public string? EndText { get; set; }
        public List<FileTreeNode>? Children { get; set; }
    }

    public sealed record DeleteFileRequest(string RelativePath);

    private string GetProtectedFilesPath()
    {
        var basePath = configuration["ProtectedFilesPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "ProtectedFiles");
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }
        return basePath;
    }

    [HttpGet("tree")]
    public IActionResult GetFileTree()
    {
        var basePath = GetProtectedFilesPath();
        var rootDir = new DirectoryInfo(basePath);
        List<FileTreeNode> items = [];

        foreach (var dir in rootDir.GetDirectories())
        {
            items.Add(BuildTreeFromDirectory(basePath, dir));
        }

        foreach (var file in rootDir.GetFiles())
        {
            items.Add(new FileTreeNode
            {
                Text = file.Name,
                Value = Path.GetRelativePath(basePath, file.FullName).Replace('\\', '/'),
                IsDirectory = false,
                EndText = FormatBytes(file.Length)
            });
        }

        return Ok(items);
    }

    [HttpGet("download/*relativePath")]
    public IActionResult DownloadFile(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return BadRequest("Invalid file path");

        var basePath = GetProtectedFilesPath();
        var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));

        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Access denied");

        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found");

        var fileName = Path.GetFileName(fullPath);
        var fileBytes = System.IO.File.ReadAllBytes(fullPath);
        return File(fileBytes, "application/octet-stream", fileName);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("upload")]
    [RequestSizeLimit(200 * 1024 * 1024)] // ~200MB limit
    public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0) return BadRequest("No files provided");

        var basePath = GetProtectedFilesPath();
        long maxSize = 200 * 1024 * 1024;
        int filesUploaded = 0;

        foreach (var file in files)
        {
            if (file.Length > maxSize)
            {
                return BadRequest($"{file.FileName} is too big to upload! Max size: 200 MB");
            }

            var safeFileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(basePath, safeFileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            filesUploaded++;
        }

        return Ok(new { message = $"Uploaded {filesUploaded} file(s)" });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("delete")]
    public IActionResult DeleteFile([FromBody] DeleteFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RelativePath)) return BadRequest("Invalid file path");

        var basePath = GetProtectedFilesPath();
        var fullPath = Path.GetFullPath(Path.Combine(basePath, request.RelativePath));

        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Access denied");

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
            return Ok(new { message = $"Removed file '{request.RelativePath}'" });
        }

        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
            return Ok(new { message = $"Removed directory '{request.RelativePath}'" });
        }

        return NotFound("File or directory not found");
    }

    private static FileTreeNode BuildTreeFromDirectory(string basePath, DirectoryInfo dir)
    {
        var folderItem = new FileTreeNode
        {
            Text = dir.Name,
            Value = Path.GetRelativePath(basePath, dir.FullName).Replace('\\', '/'),
            IsDirectory = true,
            Children = []
        };

        foreach (var subDir in dir.GetDirectories())
        {
            folderItem.Children.Add(BuildTreeFromDirectory(basePath, subDir));
        }

        foreach (var file in dir.GetFiles())
        {
            folderItem.Children.Add(new FileTreeNode
            {
                Text = file.Name,
                Value = Path.GetRelativePath(basePath, file.FullName).Replace('\\', '/'),
                IsDirectory = false,
                EndText = FormatBytes(file.Length)
            });
        }

        return folderItem;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes >= 1_000_000) return $"{bytes / 1_000_000.0:F1} MB";
        if (bytes >= 1_000) return $"{bytes / 1_000.0:F1} KB";
        return $"{bytes} B";
    }
}