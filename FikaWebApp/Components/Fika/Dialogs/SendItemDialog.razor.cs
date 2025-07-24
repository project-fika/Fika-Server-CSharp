using FikaWebApp.Models;
using FikaWebApp.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Dialogs
{
    public partial class SendItemDialog
    {
        [Inject]
        private ItemCacheService ItemCacheService { get; set; } = default!;

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        private readonly SendItemModel _model = new();
        private TimeSpan? _time;
        private bool _searching;
        private string _img = _defaultImg;
        private string? _url;
        private string _itemDescription = "Select an item";

        private static readonly string _defaultImg = "images/missing_item.png";

        private bool CanConfirm
        {
            get
            {
                if (string.IsNullOrEmpty(_model.ItemName))
                {
                    return true;
                }

                return _model.UseDate && (_model.Date == null || _time == null);
            }
        }

        private void Confirm()
        {
            _model.Date += _time;
            MudDialog.Close(_model);
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private bool CheckDate(DateTime dt)
        {
            return dt < DateTime.Now.AddDays(-1);
        }

        private async Task<IEnumerable<string>> Search(string value, CancellationToken token)
        {
            _searching = true;

            if (string.IsNullOrEmpty(value))
            {
                _searching = false;
                return ItemCacheService.Items.Values;
            }

            var result = await Task.Run(() => ItemCacheService.NameToIdSearch(value), token);

            _searching = false;
            return result;
        }

        private void GetImageAndUrl(string args)
        {
            _model.ItemName = args;
            if (string.IsNullOrEmpty(args))
            {
                _img = _defaultImg;
                _url = null;
                return;
            }

            var tpl = ItemCacheService.NameToId(args);
            if (Statics.IsValidMongoId(tpl))
            {
                _img = $"https://assets.tarkov.dev/{tpl}-icon.webp";
                _url = $"https://tarkov.dev/item/{tpl}";
                _model.TemplateId = tpl;
                _itemDescription = ItemCacheService.GetDescription(tpl);
                StateHasChanged();
            }
        }
    }
}