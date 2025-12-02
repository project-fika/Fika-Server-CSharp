using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using System.Text.RegularExpressions;

namespace FikaWebApp.Components.Fika.Dialogs;

public partial class AddAccountDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; }

    private IEnumerable<string> _selectedRoles = new HashSet<string>();
    private string[] _availableRoles = [];
    private string _roleValue = "None";

    private bool _success;
    private string[] _errors = { };
    private MudTextField<string> _usernameField;
    private MudTextField<string> _passwordField;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _availableRoles = RoleManager.Roles
            .Select(r => r.Name)
            .ToArray();
    }

    private void Submit()
    {
        Tuple<string, string, List<string>> value = new(_usernameField.Value, _passwordField.Value, [.. _selectedRoles]);
        MudDialog.Close(value);
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private IEnumerable<string> UsernameLength(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            yield return "Username is required!";
            yield break;
        }

        if (username.Length < 5)
        {
            yield return "Username must be at least of length 5";
        }

        if (username.Length > 10)
        {
            yield return "Username can not be longer than 10!";
        }
    }

    private IEnumerable<string> PasswordStrength(string pw)
    {
        var pwOptions = IdentityOptions.Value.Password;

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

    private string? PasswordMatch(string arg)
    {
        if (_passwordField.Value != arg)
        {
            return "Passwords don't match";
        }

        return null;
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