using System.Globalization;
using System.Net;
using FikaShared.Requests;
using FikaShared.Responses;
using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Models;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages;

public partial class ToolsPage
{
    [Inject]
    private HttpClient HttpClient { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private SendTimersService SendTimersService { get; set; } = default!;

    [Inject]
    private ItemCacheService ItemCacheService { get; set; } = default!;

    [Inject]
    private ILogger<ToolsPage> Logger { get; set; } = default!;

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
                    var serverProfiles = await HttpClient.GetFromJsonAsync<List<ProfileResponse>>("/fika/api/profiles");
                    profiles.AddRange(serverProfiles);
                }
                catch (HttpRequestException httpEx)
                {
                    if (httpEx.StatusCode is HttpStatusCode.Forbidden)
                    {
                        Snackbar.Add("Something went wrong when sending the item: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                        Logger.LogError("Something went wrong when sending the item: [403 Forbidden]. Are you using the wrong API key?");
                    }
                    else if (httpEx.StatusCode is HttpStatusCode.NotFound)
                    {
                        Snackbar.Add("Something went wrong when sending the item: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                        Logger.LogError("Something went wrong when sending the item: [404 NotFound]. Are you missing the Fika server mod?");
                    }
                    else
                    {
                        Snackbar.Add($"There was a HttpRequestException caught when when sending the item:\n{httpEx.Message}", Severity.Error);
                        Logger.LogError("There was a HttpRequestException caught when when sending the item: {HttpException}", httpEx.Message);
                    }
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
                        await HttpClient.PostAsJsonAsync("fika/api/senditemtoall", request);
                        Snackbar.Add("The item was sent to everyone.", Severity.Success);
                    }
                    catch (HttpRequestException httpEx)
                    {
                        if (httpEx.StatusCode is HttpStatusCode.Forbidden)
                        {
                            Snackbar.Add("Something went wrong when sending the item: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                            Logger.LogError("Something went wrong when sending the item: [403 Forbidden]. Are you using the wrong API key?");
                        }
                        else if (httpEx.StatusCode is HttpStatusCode.NotFound)
                        {
                            Snackbar.Add("Something went wrong when sending the item: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                            Logger.LogError("Something went wrong when sending the item: [404 NotFound]. Are you missing the Fika server mod?");
                        }
                        else
                        {
                            Snackbar.Add($"There was a HttpRequestException caught when when sending the item:\n{httpEx.Message}", Severity.Error);
                            Logger.LogError("There was a HttpRequestException caught when when sending the item: {HttpException}", httpEx.Message);
                        }
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

    private async Task RefreshItemDatabase(MouseEventArgs arg)
    {
        var success = await ItemCacheService.PopulateDictionary();
        if (success)
        {
            Snackbar.Add("Items successfully refreshed", Severity.Success);
        }
        else
        {
            Snackbar.Add("There was an error refreshing the database", Severity.Error);
        }
    }
}