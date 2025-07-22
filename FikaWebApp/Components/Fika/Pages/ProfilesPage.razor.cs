using FikaShared.Responses;
using FikaWebApp.Components.Fika.Dialogs;
using MudBlazor;
using MudExtensions;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class ProfilesPage
    {
        MudDataGrid<ProfileResponse> _dataGrid;
        private string _searchString = null;

        private async Task OpenModifyDialog(ProfileResponse row)
        {
            var parameters = new DialogParameters
        {
            { "Profile", row }
        };

            var options = new DialogOptions
            {
                BackgroundClass = "blur-background"
            };

            var res = await DialogService.ShowAsync<ModifyProfileDialog>("Modify Profile", parameters, options);
            var result = await res.Result;

            if (!result.Canceled)
            {
                StateHasChanged();
            }
        }

        private async Task<GridData<ProfileResponse>> ServerReload(GridState<ProfileResponse> state)
        {
            IEnumerable<ProfileResponse> data;
            try
            {
                data = await HttpClient.GetFromJsonAsync<List<ProfileResponse>>("get/profiles");
            }
            catch (Exception ex)
            {
                Logger.LogError($"There was an error retrieving the profiles: {ex.Message}");
                Snackbar.Add($"An error occured when querying: {ex.Message}", Severity.Error);
                data = [];
            }

            data = data.Where(element =>
            {
                if (string.IsNullOrWhiteSpace(_searchString))
                    return true;
                if (element.Nickname.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (element.ProfileId.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }).ToArray();

            var totalItems = data.Count();

            var sortDefinition = state.SortDefinitions.FirstOrDefault();
            if (sortDefinition != null)
            {
                switch (sortDefinition.SortBy)
                {
                    case nameof(ProfileResponse.Nickname):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.Nickname
                        );
                        break;
                    case nameof(ProfileResponse.ProfileId):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.ProfileId
                        );
                        break;
                    case nameof(ProfileResponse.HasFleaBan):
                        data = data.OrderByDirection(
                            sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                            o => o.HasFleaBan
                        );
                        break;
                    default:
                        var sortByColumn = _dataGrid.RenderedColumns.First(c => c.PropertyName == sortDefinition.SortBy);
                        switch (sortByColumn.Title)
                        {
                            case nameof(ProfileResponse.Nickname):
                                data = data.OrderByDirection(
                                    sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending,
                                    o => o.Nickname
                                );
                                break;
                        }
                        break;
                }
            }

            var pagedData = data.Skip(state.Page * state.PageSize)
                .Take(state.PageSize)
                .ToArray();

            return new()
            {
                TotalItems = totalItems,
                Items = pagedData
            };
        }

        private Task OnSearch(string text)
        {
            _searchString = text;
            return _dataGrid.ReloadServerData();
        }
    }
}