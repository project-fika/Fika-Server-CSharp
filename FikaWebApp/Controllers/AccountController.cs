using System.Security.Claims;
using FikaWebApp.Data;
using FikaWebApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public sealed class AccountController(UserManager<ApplicationUser> userManager) : ControllerBase
{
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new ApiResponseDto("User not found"));
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiResponseDto(string.Join(", ", result.Errors.Select(e => e.Description))));
        }

        return Ok(new ApiResponseDto("Password updated successfully"));
    }
}