using FikaWebApp.Data;
using FikaWebApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FikaWebApp.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public sealed class AccountsController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : ControllerBase
{
    public sealed record UserDto(string Id, string UserName, List<string> Roles, DateTimeOffset? LockoutEnd);
    public sealed record CreateUserRequest(string Username, string Password, List<string> Roles);
    public sealed record UpdateRolesRequest(List<string> Roles);
    public sealed record ResetPasswordRequest(string NewPassword);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await userManager.Users.ToListAsync();
        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = (await userManager.GetRolesAsync(user)).ToList();
            result.Add(new UserDto(user.Id, user.UserName!, roles, user.LockoutEnd));
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        var existing = await userManager.FindByNameAsync(request.Username);
        if (existing != null)
        {
            return BadRequest(new ApiResponseDto($"An account with the username '{request.Username}' already exists!"));
        }

        var newUser = new ApplicationUser { UserName = request.Username };
        var createResult = await userManager.CreateAsync(newUser, request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(new ApiResponseDto(string.Join(", ", createResult.Errors.Select(e => e.Description))));
        }

        if (request.Roles != null && request.Roles.Count != 0)
        {
            await userManager.AddToRolesAsync(newUser, request.Roles);
        }

        return Ok(new ApiResponseDto($"User '{newUser.UserName}' was created!"));
    }

    [HttpPut("{id}/roles")]
    public async Task<ActionResult<ApiResponseDto>> UpdateUserRoles(string id, [FromBody] UpdateRolesRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new ApiResponseDto("User not found"));
        }

        if (user.UserName == User.Identity?.Name)
        {
            return BadRequest(new ApiResponseDto("You cannot modify your own account!"));
        }

        if (user.UserName == "admin")
        {
            return BadRequest(new ApiResponseDto("You cannot modify the root user"));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);

        var addResult = await userManager.AddToRolesAsync(user, request.Roles);
        if (!addResult.Succeeded)
        {
            return BadRequest(new ApiResponseDto(string.Join(", ", addResult.Errors.Select(e => e.Description))));
        }

        return Ok(new ApiResponseDto($"Modified roles on {user.UserName}"));
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<ApiResponseDto>> ResetUserPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new ApiResponseDto("User not found"));
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new ApiResponseDto(string.Join(", ", result.Errors.Select(e => e.Description))));
        }

        return Ok(new ApiResponseDto($"Password reset successfully for '{user.UserName}'!"));
    }

    [HttpPost("{id}/toggle-lock")]
    public async Task<ActionResult<ApiResponseDto>> ToggleLockUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new ApiResponseDto("User not found"));
        }

        if (user.UserName == User.Identity?.Name)
        {
            return BadRequest(new ApiResponseDto("You cannot lock your own account!"));
        }

        if (user.UserName == "admin")
        {
            return BadRequest(new ApiResponseDto("You cannot lock the root account!"));
        }

        IdentityResult result;
        if (!user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow)
        {
            result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
        }
        else
        {
            result = await userManager.SetLockoutEndDateAsync(user, null);
        }

        if (!result.Succeeded)
        {
            return BadRequest(new ApiResponseDto("Failed to toggle account lock!"));
        }

        return Ok(new ApiResponseDto("Account lock was toggled successfully!"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteUser(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new ApiResponseDto("User not found"));
        }

        if (user.UserName == User.Identity?.Name)
        {
            return BadRequest(new ApiResponseDto("You cannot delete your own account!"));
        }

        if (user.UserName == "admin")
        {
            return BadRequest(new ApiResponseDto("You cannot delete the root account!"));
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiResponseDto($"User '{user.UserName}' could not be removed!"));
        }

        return Ok(new ApiResponseDto($"User '{user.UserName}' was removed successfully!"));
    }
}