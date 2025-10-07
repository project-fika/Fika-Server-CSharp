using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages;

public partial class AccountsPage
{
    private string _username;

    private MudDataGrid<ApplicationUser> _dataGrid;
    private IEnumerable<ApplicationUser> _users;

    private async Task<IEnumerable<string>> GetRoles(ApplicationUser user)
    {
        var roles = await UserManager.GetRolesAsync(user);
        return roles;
    }

    private string? GetLockText(ApplicationUser user)
    {
        if (user.LockoutEnd.HasValue)
        {
            return $"Expires at {user.LockoutEnd.Value:G}";
        }

        return null;
    }

    private async Task EditUser(ApplicationUser user)
    {
        if (user == null)
        {
            return;
        }

        if (user.UserName == _username)
        {
            Snackbar.Add($"You cannot modify your own account!", Severity.Warning);
            return;
        }

        if (user.UserName == "admin")
        {
            Snackbar.Add("You cannot modify the root user", Severity.Warning);
            return;
        }

        var parameters = new DialogParameters()
        {
            { "User", user }
        };

        var dialog = await DialogService.ShowAsync<ModifyAccountDialog>("Modify Account", parameters);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            if (result.Data is List<string> roles)
            {
                var currentRoles = await UserManager.GetRolesAsync(user);
                var removeResult = await UserManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    Snackbar.Add($"An unknown error has occurred", Severity.Error);
                    return;
                }

                var changeResult = await UserManager.AddToRolesAsync(user, roles);
                if (changeResult.Succeeded)
                {
                    Snackbar.Add($"Modified roles on {user.UserName}", Severity.Success);
                }
                else
                {
                    Snackbar.Add($"There was an error modifying the roles: {string.Join(", ", changeResult.Errors)}");
                }
            }
        }
    }

    private async Task DeleteUser(ApplicationUser user)
    {
        if (user.UserName == _username)
        {
            Snackbar.Add($"You cannot delete your own account!", Severity.Warning);
            return;
        }

        if (user.UserName == "admin")
        {
            Snackbar.Add($"You cannot delete the root account!", Severity.Warning);
            return;
        }

        var confirmationDialog = await DialogService.ShowAsync<YesNoDialog>("Confirmation");
        var confirmationResult = await confirmationDialog.Result;
        if (confirmationResult.Canceled)
        {
            return;
        }

        var result = await UserManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            Snackbar.Add($"User '{user.UserName}' was removed successfully!", Severity.Success);
        }
        else
        {
            Snackbar.Add($"User '{user.UserName}' could not be removed!", Severity.Error);
        }

        RefreshUsers();
    }

    private async Task ToggleLockUser(ApplicationUser user)
    {
        if (user.UserName == _username)
        {
            Snackbar.Add($"You cannot lock your own account!", Severity.Warning);
            return;
        }

        if (user.UserName == "admin")
        {
            Snackbar.Add($"You cannot lock the root account!", Severity.Warning);
            return;
        }

        IdentityResult? result;
        if (!user.LockoutEnd.HasValue)
        {
            result = await UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100));
        }
        else
        {
            result = await UserManager.SetLockoutEndDateAsync(user, null);
        }

        if (!result.Succeeded)
        {
            Snackbar.Add($"Failed to toggle account lock!", Severity.Error);
        }
        else
        {
            Snackbar.Add($"Account lock was toggled successfully!", Severity.Success);
        }

        RefreshUsers();
    }

    private async Task AddUser()
    {
        DialogOptions options = new() { MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<AddAccountDialog>("Add Account", options);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            if (result.Data is (string username, string password, List<string> roles))
            {
                if (await UserManager.FindByNameAsync(username) == null)
                {
                    var newUser = new ApplicationUser()
                    {
                        UserName = username
                    };

                    var createResult = await UserManager.CreateAsync(newUser, password);
                    if (!createResult.Succeeded)
                    {
                        Snackbar.Add("Unable to create user!", Severity.Error);
                    }
                    else
                    {
                        Snackbar.Add($"User '{newUser.UserName}' was created!", Severity.Success);
                    }

                    newUser = await UserManager.FindByNameAsync(username);
                    if (newUser != null)
                    {
                        await UserManager.AddToRolesAsync(newUser, roles);
                    }

                    RefreshUsers();
                }
                else
                {
                    Snackbar.Add($"An account with the username '{username}' already exists!", Severity.Warning);
                }
            }
        }
    }

    private void RefreshUsers()
    {
        _users = UserManager.Users.ToList();
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            _username = user.Identity.Name!;
        }
        else
        {
            _username = string.Empty;
        }

        RefreshUsers();
    }
}