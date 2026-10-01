using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FikaWebApp.Data;
using FikaWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FikaWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IConfiguration configuration) : ControllerBase
{
    public sealed record LoginRequest(string Username, string Password);
    public sealed record AuthResponse(string Token, string Username, string Id);

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            return Unauthorized(new ApiResponseDto("Invalid credentials"));
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new ApiResponseDto("Invalid credentials"));
        }

        var token = await GenerateJwtTokenAsync(user);
        return Ok(new AuthResponse(token, user.UserName!, user.Id));
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"]
            ?? "SUPER_SECRET_KEY_CHANGE_IN_PRODUCTION_MINIMUM_32_CHARS!";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(jwtSecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? "")
        };

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}