using FikaShared.Requests;
using FikaShared.Responses;
using FikaWebApp.Models;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace FikaWebApp.Components.Fika.Dialogs;

public partial class ModifyProfileDialog
{
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private HttpClient HttpClient { get; set; } = default!;

    [Inject]
    private SendTimersService SendTimersService { get; set; } = default!;

    [CascadingParameter]
    public IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public ProfileResponse Profile { get; set; } = default!;

    [Inject]
    private ILogger<ModifyProfileDialog> Logger { get; set; } = default!;


    private async Task SendItem()
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
                SendItemRequest sendItemRequest = new()
                {

                    ProfileId = Profile.ProfileId,
                    ItemTemplate = model.TemplateId,
                    Amount = model.Amount,
                    Message = model.Message,
                    FoundInRaid = model.FoundInRaid,
                    ExpirationDays = model.ExpirationDays
                };

                if (model.UseDate)
                {
                    SendTimersService.AddTimer(sendItemRequest, model.Date.Value);
                }
                else
                {
                    try
                    {
                        var postResult = await HttpClient.PostAsJsonAsync("fika/api/senditem", sendItemRequest);
                        if (postResult.IsSuccessStatusCode)
                        {
                            Snackbar.Add($"[{model.ItemName}] was successfully sent to {Profile.Nickname}", Severity.Success);
                        }
                        else
                        {
                            var errorMessage = await postResult.Content.ReadAsStringAsync();
                            Snackbar.Add($"There was an error sending the item: [{postResult.StatusCode}] {errorMessage}", Severity.Error);
                        }
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
                        Snackbar.Add($"There was an error sending the item: {ex.Message}", Severity.Error);
                    }
                }
            }
        }
    }

    private async Task AddFleaBan()
    {
        var confirmation = await DialogService.ShowAsync<ConfirmFleaBanDialog>("Confirm Flea Ban");
        var result = await confirmation.Result;

        if (!result.Canceled)
        {
            if (result.Data is int amountOfDays)
            {
                amountOfDays = Math.Clamp(amountOfDays, 0, 9999);
                var response = await HttpClient.PostAsJsonAsync("fika/api/fleaban", new AddFleaBanRequest()
                {
                    ProfileId = Profile.ProfileId,
                    AmountOfDays = amountOfDays
                });

                Profile.HasFleaBan = response.StatusCode is HttpStatusCode.OK;
            }
        }
    }

    private async Task RemoveFleaBan()
    {
        var confirmation = await DialogService.ShowAsync<YesNoDialog>("Confirmation");
        var result = await confirmation.Result;

        if (!result.Canceled)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("fika/api/fleaban", UriKind.Relative),
                Content = JsonContent.Create(new ProfileIdRequest
                {
                    ProfileId = Profile.ProfileId
                })
            };

            var response = await HttpClient.SendAsync(request);

            if (response.StatusCode is HttpStatusCode.OK)
            {
                Profile.HasFleaBan = false;
            }
        }
    }

    private void Close()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}