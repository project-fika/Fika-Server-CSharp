using FikaShared.Responses;
using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MudBlazor;
using MudExtensions;
using System;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class ProfilesPage
    {
        [Inject] 
        UserManager<ApplicationUser> UserManager { get; set; }

        [Inject]
        HttpClient HttpClient { get; set; }

        [Inject]
        ILogger<ProfilesPage> Logger { get; set; }

        [Inject]
        IDialogService DialogService { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        MudDataGrid<ProfileResponse> _dataGrid;
        private string _searchString = null;
        private List<ProfileResponse> _profiles = [];
        private IList<IBrowserFile> _files = [];

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                var result = await HttpClient.GetFromJsonAsync<List<ProfileResponse>>("get/profiles");
                _profiles.AddRange(result);
            }
            catch (Exception ex)
            {
                Logger.LogError("There was an error retrieving the profiles: {ExceptionMessage}", ex.Message);
                Snackbar.Add($"An error occured when querying: {ex.Message}", Severity.Error);
            }
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
                    { "RawJson", result }
                };
                var options = new DialogOptions()
                {
                    MaxWidth = MaxWidth.Large,
                    FullWidth = true,
                    Position = DialogPosition.Center
                };

                await DialogService.ShowAsync<ViewRawProfileDialog>($"Viewing {row.Nickname}", parameters, options);
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

            if (!result.Canceled)
            {
                StateHasChanged();
            }
        }
		private async Task UploadProfile(IReadOnlyList<IBrowserFile> files)
		{
			if (_files.Count > 1)
            {
                Snackbar.Add("Too many files!", Severity.Warning);
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
                            Snackbar.Add("The profile was successfully uploaded", Severity.Success);
                        }
                        else
                        {
                            var errorMessage = await result.Content.ReadAsStringAsync();
                            Snackbar.Add($"There was an error uploading the profile: {errorMessage}", Severity.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"There was an error reading the profile: {ex.Message}", Severity.Error);
            }
		}
    }
}