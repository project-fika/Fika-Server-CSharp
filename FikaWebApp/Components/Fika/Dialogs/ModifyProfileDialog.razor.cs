using FikaShared.Requests;
using FikaShared.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace FikaWebApp.Components.Fika.Dialogs
{
    public partial class ModifyProfileDialog
    {
        [Inject]
        private IDialogService DialogService { get; set; }
        [Inject]
        private ISnackbar Snackbar { get; set; }
        [Inject]
        private HttpClient HttpClient { get; set; }

        [CascadingParameter]
        public IMudDialogInstance MudDialog { get; set; }

        [Parameter]
        public ProfileResponse Profile { get; set; }


        private async Task SendItem()
        {
            var dialog = await DialogService.ShowAsync<SendItemDialog>("Send Item");
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                if (result.Data is (string itemTpl, int amount, string message))
                {
                    SendItemRequest sendItemRequest = new()
                    {
                        ProfileId = Profile.ProfileId,
                        ItemTemplate = itemTpl,
                        Amount = amount,
                        Message = message
                    };

                    try
                    {
                        var postResult = await HttpClient.PostAsJsonAsync("post/senditem", sendItemRequest);
                        if (postResult.IsSuccessStatusCode)
                        {
                            Snackbar.Add($"Item was successfully sent to {Profile.Nickname}", Severity.Success);
                        }
                        else
                        {
                            var errorMessage = await postResult.Content.ReadAsStringAsync();
                            Snackbar.Add($"There was an error sending the item: [{postResult.StatusCode}] {errorMessage}", Severity.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Snackbar.Add($"There was an error sending the item: {ex.Message}", Severity.Error);
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
                    var response = await HttpClient.PostAsJsonAsync("post/addfleaban", new AddFleaBanRequest()
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
                var response = await HttpClient.PostAsJsonAsync("post/removefleaban", new ProfileIdRequest()
                {
                    ProfileId = Profile.ProfileId
                });

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
}