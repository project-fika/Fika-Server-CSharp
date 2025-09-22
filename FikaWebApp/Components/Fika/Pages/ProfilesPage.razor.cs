using FikaShared.Responses;
using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages;

public partial class ProfilesPage
{
    [Inject]
    UserManager<ApplicationUser> UserManager { get; set; } = default!;

    [Inject]
    HttpClient HttpClient { get; set; } = default!;

    [Inject]
    ILogger<ProfilesPage> Logger { get; set; } = default!;

    [Inject]
    IDialogService DialogService { get; set; } = default!;

    [Inject]
    ISnackbar Snackbar { get; set; } = default!;

    MudDataGrid<ProfileResponse> _dataGrid = default!;
    private string _searchString = null!;
    private readonly List<ProfileResponse> _profiles = [];
    private readonly IList<IBrowserFile> _files = [];
    private bool _loading;

    private Func<ProfileResponse, bool> _quickFilter
    {
        get
        {
            return x =>
            {
                if (string.IsNullOrWhiteSpace(_searchString))
                {
                    return true;
                }

                return x.Nickname.Contains(_searchString, StringComparison.OrdinalIgnoreCase)
                    || x.ProfileId.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
            };
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await RefreshProfiles();
    }

    public async Task RefreshProfiles()
    {
        _loading = true;
        _profiles.Clear();
        try
        {
            var result = await HttpClient.GetFromJsonAsync<List<ProfileResponse>>("get/profiles");
            _profiles.AddRange(result!);
        }
        catch (Exception ex)
        {
            Logger.LogError("There was an error retrieving the profiles: {ExceptionMessage}", ex.Message);
            Snackbar.Add($"An error occured when querying: {ex.Message}", Severity.Error);
        }
        _loading = false;
    }

    private async Task ViewProfile(ProfileResponse row)
    {
        try
        {
            var result = await HttpClient.GetStringAsync($"get/rawprofile?profileId={Uri.EscapeDataString(row.ProfileId)}")
                ?? throw new NullReferenceException("The data was empty");

            /*var jsonElement = JsonSerializer.Deserialize<JsonElement>(result);
            string formattedJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });*/

            var parameters = new DialogParameters()
            {
                { "RawJson", result },
                { "FileName", row.ProfileId }
            };
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                Position = DialogPosition.Center
            };

            await DialogService.ShowAsync<ViewJsonDialog>($"Viewing {row.Nickname}", parameters, options);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"There was an error retrieving the data: {ex.Message}", Severity.Error);
        }
    }

    private async Task OpenModifyDialog(ProfileResponse row)
    {
        var parameters = new DialogParameters
        {
            { "Profile", row }
        };

        var options = new DialogOptions
        {
            BackgroundClass = "blur-background",
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var res = await DialogService.ShowAsync<ModifyProfileDialog>($"Modify {row.Nickname}", parameters, options);
        var result = await res.Result;

        if (!result!.Canceled)
        {
            StateHasChanged();
        }
    }
    private async Task UploadProfile(IReadOnlyList<IBrowserFile> files)
    {
        if (_files.Count > 1)
        {
            Snackbar.Add("Too many files! You can only upload one profile", Severity.Warning);
            return;
        }

        var options = new MessageBoxOptions()
        {
            Title = "WARNING",
            MarkupMessage = new("This is an experimental feature.<br/>Are you sure? Damage might be irreversible!"),
            YesText = "YES",
            CancelText = "NO"
        };
        var confirmation = await DialogService.ShowMessageBox(options);
        if (!confirmation.HasValue)
        {
            return;
        }

        try
        {
            var file = files[0];
            using (var stream = file.OpenReadStream(10 * 1024 * 1024))
            {
                using (var fs = new StreamReader(stream))
                {
                    var data = await fs.ReadToEndAsync();
                    var result = await HttpClient.PostAsync("post/uploadprofile", new StringContent(data));

                    if (result.IsSuccessStatusCode)
                    {
                        var message = await result.Content.ReadAsStringAsync();
                        Snackbar.Add(message, Severity.Success);
                    }
                    else
                    {
                        var errorMessage = await result.Content.ReadAsStringAsync();
                        Snackbar.Add($"There was an error uploading the profile: {errorMessage}", Severity.Error);
                    }
                }
            }

            await RefreshProfiles();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"There was an error reading the profile: {ex.Message}", Severity.Error);
        }
    }
}