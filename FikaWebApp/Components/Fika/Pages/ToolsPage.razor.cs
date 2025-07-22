using FikaShared.Requests;
using FikaShared.Responses;
using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
            var dialog = await DialogService.ShowAsync<SendItemDialog>("Send Item To All");
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
                        Message = model.Message
                    };

                    if (model.UseDate)
                    {
                        SendTimersService.AddTimer(request, model.Date.Value);
                    }
                    else
                    {
                        try
                        {
                            await HttpClient.PostAsJsonAsync("post/senditemtoall", request);
                        }
                        catch (Exception ex)
                        {
                            Snackbar.Add($"There was an error sending an item to all players: {ex.Message}", Severity.Error);
                        }
                    }
                }
            }
        }
    }
}