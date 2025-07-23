using FikaShared.Requests;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Dialogs
{
    public partial class ViewQueuedSendItemsDialog
    {
        [Inject]
        private SendTimersService SendTimersService { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        public async Task DeleteTimer(KeyValuePair<Timer, SendItemRequest> row)
        {
            var dialog = await DialogService.ShowAsync<YesNoDialog>("Confirmation");
            var result = await dialog.Result;
            if (result.Canceled)
            {
                return;
            }

            SendTimersService.RemoveTimer(row.Key);
        }

        public async Task DeleteTimer(KeyValuePair<Timer, SendItemToAllRequest> row)
        {
            var dialog = await DialogService.ShowAsync<YesNoDialog>("Confirmation");
            var result = await dialog.Result;
            if (result.Canceled)
            {
                return;
            }

            SendTimersService.RemoveTimer(row.Key);
        }
    }
}