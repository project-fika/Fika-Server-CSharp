using FikaWebApp.Components.Fika.Pages;
using FikaWebApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using System.Text.RegularExpressions;

namespace FikaWebApp.Components.Fika.Dialogs;

public partial class ModifyAccountDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    [Parameter]
    public ApplicationUser User { get; set; }

    [Inject]
    private ILogger<ModifyAccountDialog> Logger { get; set; } = default!;

    private IEnumerable<string> _selectedRoles = new HashSet<string>();
    private string[] _availableRoles = [];
    private string _roleValue = "None";
    private MudTextField<string> _passwordField;

    private bool _success;
    private string[] _errors = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _availableRoles = [.. RoleManager.Roles.Select(r => r.Name)];

        var roles = await UserManager.GetRolesAsync(User);
        _selectedRoles = new HashSet<string>(roles);
    }

    private void Submit()
    {
        MudDialog.Close(_selectedRoles.ToList());
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

	private async Task ChangePassword()
	{
        var token = await UserManager.GeneratePasswordResetTokenAsync(User);
        var result = await UserManager.ResetPasswordAsync(User, token, _passwordField.Value);

        if (result.Succeeded)
        {
            Snackbar.Add($"Successfully changed the password of {User.NormalizedUserName}", Severity.Success);
            return;
        }

        Snackbar.Add($"Failed to change the password of {User.NormalizedUserName}", Severity.Error);
        foreach (var error in result.Errors)
        {
            Logger.LogError("Error changing password of {0}: {1}", User.NormalizedUserName, error.Description);
        }
	}

    private string? PasswordMatch(string arg)
    {
        if (_passwordField.Value != arg)
        {
            return "Passwords don't match";
        }

        return null;
    }

    private IEnumerable<string> PasswordStrength(string pw)
    {
        var pwOptions = IdentityOptions.Value.Password; // or IdentityOptionsAccessor.Value.Password in Blazor

        if (string.IsNullOrWhiteSpace(pw))
        {
            yield return "Password is required!";
            yield break;
        }

        if (pw.Length < pwOptions.RequiredLength)
        {
            yield return $"Password must be at least {pwOptions.RequiredLength} characters long";
        }

        if (pwOptions.RequireUppercase && !UpperCaseRegex().IsMatch(pw))
        {
            yield return "Password must contain at least one uppercase letter";
        }

        if (pwOptions.RequireLowercase && !LowerCaseRegex().IsMatch(pw))
        {
            yield return "Password must contain at least one lowercase letter";
        }

        if (pwOptions.RequireDigit && !DigitsRegex().IsMatch(pw))
        {
            yield return "Password must contain at least one digit";
        }

        if (pwOptions.RequireNonAlphanumeric && !NonAlphaRegex().IsMatch(pw))
        {
            yield return "Password must contain at least one special character";
        }

        if (pwOptions.RequiredUniqueChars > 0 && pw.Distinct().Count() < pwOptions.RequiredUniqueChars)
        {
            yield return $"Password must contain at least {pwOptions.RequiredUniqueChars} unique characters";
        }
    }

    [GeneratedRegex(@"[0-9]")]
    private static partial Regex DigitsRegex();
    [GeneratedRegex(@"[\W_]")]
    private static partial Regex NonAlphaRegex();
    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowerCaseRegex();
    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UpperCaseRegex();
}