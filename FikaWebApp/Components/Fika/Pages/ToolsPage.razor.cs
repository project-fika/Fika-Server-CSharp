using FikaShared.Requests;
using FikaShared.Responses;
using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Models;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using static FikaWebApp.Components.Fika.Dialogs.SendItemDialog;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class ToolsPage
    {
        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Inject]
        private SendTimersService SendTimersService { get; set; }

        private async Task SendItemToEveryone()
        {
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                NoHeader = true
            };
            var dialog = await DialogService.ShowAsync<SendItemDialog>(string.Empty, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                if (result.Data is SendItemModel model)
                {
                    if (model.Date < DateTime.Now)
                    {
                        Snackbar.Add("You cannot send items to the past!", Severity.Error);
                        return;
                    }

                    List<ProfileResponse> profiles = [];
                    try
                    {
                        var serverProfiles = await HttpClient.GetFromJsonAsync<List<ProfileResponse>>("get/profiles");
                        profiles.AddRange(serverProfiles);
                    }
                    catch (Exception ex)
                    {
                        Snackbar.Add($"There was an error receiving all profiles: {ex.Message}", Severity.Error);
                        return;
                    }

                    SendItemToAllRequest request = new()
                    {
                        ProfileIds = [.. profiles.Select(p => p.ProfileId)],
                        ItemTemplate = model.TemplateId,
                        Amount = model.Amount,
                        Message = model.Message,
                        FoundInRaid = model.FoundInRaid,
                        ExpirationDays = model.ExpirationDays
                    };

                    if (model.UseDate)
                    {
                        SendTimersService.AddTimer(request, model.Date.Value);
                        Snackbar.Add($"The item was queued to be sent to everyone at {model.Date.Value.ToString(CultureInfo.CurrentCulture)}.", Severity.Success);
                    }
                    else
                    {
                        try
                        {
                            await HttpClient.PostAsJsonAsync("post/senditemtoall", request);
                            Snackbar.Add($"The item was sent to everyone.", Severity.Success);
                        }
                        catch (Exception ex)
                        {
                            Snackbar.Add($"There was an error sending an item to all players: {ex.Message}", Severity.Error);
                        }
                    }
                }
            }
        }
		private async Task OpenQueuedItems()
		{
            var options = new DialogOptions()
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true
            };
            var dialog = DialogService.ShowAsync<ViewQueuedSendItemsDialog>("Queued Items", options);
		}
    }
}