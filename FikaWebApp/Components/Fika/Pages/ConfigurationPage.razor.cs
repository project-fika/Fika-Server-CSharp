using FikaWebApp.Components.Fika.Dialogs;
using FikaWebApp.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class ConfigurationPage
    {
        [Inject]
        private IDialogService DialogService { get; set; }

        private FikaConfig _config = new();

        private List<string> _blacklistItemsChips = [];
        private string? _blacklistItemValue;

        private List<string> _requiredMods = [];
        private List<string> _optionalMods = [];
        private string? _modsValue;

        private List<string> _adminIdsChips = [];
        private string? _adminIdValue;

        private void CloseBlacklistItemChip(MudChip<string> chip)
        {
            _blacklistItemsChips.Remove(chip.Text);
        }

        private void CloseRequiredModsChip(MudChip<string> chip)
        {
            _requiredMods.Remove(chip.Text);
        }

        private void CloseOptionalModsChip(MudChip<string> chip)
        {
            _optionalMods.Remove(chip.Text);
        }

        private void CloseAdminIdChip(MudChip<string> chip)
        {
            _adminIdsChips.Remove(chip.Text);
        }

        private async Task AddBlacklistItem()
        {
            if (string.IsNullOrEmpty(_blacklistItemValue))
            {
                return;
            }

            if (!Statics.IsValidMongoId(_blacklistItemValue))
            {
                await DialogService.ShowMessageBox("Invalid", $"{_blacklistItemValue} is not a valid MongoId");
                return;
            }

            if (!string.IsNullOrEmpty(_blacklistItemValue))
            {
                _blacklistItemsChips.Add(_blacklistItemValue);
                _blacklistItemValue = string.Empty;
            }
        }

        private async Task AddMod(bool required)
        {
            if (string.IsNullOrEmpty(_modsValue))
            {
                return;
            }

            if (required)
            {
                _requiredMods.Add(_modsValue);
            }
            else
            {
                _optionalMods.Add(_modsValue);
            }

            _modsValue = string.Empty;
        }

        private async Task AddAdmin()
        {
            if (string.IsNullOrEmpty(_adminIdValue))
            {
                return;
            }

            if (!Statics.IsValidMongoId(_adminIdValue))
            {
                await DialogService.ShowMessageBox("Invalid", $"{_blacklistItemValue} is not a valid MongoId");
                return;
            }

            _adminIdsChips.Add(_adminIdValue);
            _adminIdValue = string.Empty;
        }

        private async Task AddAlias()
        {
            var options = new DialogOptions() { FullWidth = true };
            var dialog = await DialogService.ShowAsync<AddAliasDialog>("Add Alias", options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                if (result.Data is (string profileid, string alias))
                {
                    _config.Headless.Profiles.Aliases.Add(profileid, alias);
                }
            }
        }

        private async Task RemoveAlias(string profileId)
        {
            _config.Headless.Profiles.Aliases.Remove(profileId);
        }
    }
}