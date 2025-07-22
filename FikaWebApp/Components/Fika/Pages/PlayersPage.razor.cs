using FikaShared;
using FikaShared.Requests;
using FikaWebApp.Components.Fika.Dialogs;
using MudBlazor;
using static FikaShared.Enums;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class PlayersPage
    {
        private List<OnlinePlayer> _players = [];
        private bool _loading;

#if RELEASE
	protected override async Task OnInitializedAsync()
	{
		_loading = true;
		await base.OnInitializedAsync();

		try
		{
			var result = await HttpClient.GetFromJsonAsync<GetOnlinePlayersResponse>("get/players");
			if (result != null)
			{
				_players = result.Players;
			}
		}
		catch (Exception ex)
		{
			Snackbar.Add($"There was an error retrieving the players: {ex.Message}", Severity.Error);
		}

		_loading = false;
	}
#endif

        private bool IsRestricted(EFikaLocation location)
        {
            return location != EFikaLocation.None && location != EFikaLocation.Hideout;
        }

        private async Task SendMessage(OnlinePlayer? player)
        {

            if (player == null)
            {
                return;
            }

            var options = new DialogOptions()
            {
                FullWidth = true
            };
            var dialog = await DialogService.ShowAsync<SendMessageDialog>("Send Message", options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                _loading = true;
                StateHasChanged();
                if (result.Data is string message)
                {
                    SendMessageRequest request = new()
                    {
                        ProfileId = player.ProfileId,
                        Message = message
                    };

                    try
                    {
                        var postResult = await HttpClient.PostAsJsonAsync("post/sendmessage", request);
                        Snackbar.Add($"Message sent to {player.Nickname}", Severity.Success);
                    }
                    catch (Exception ex)
                    {
                        Snackbar.Add($"Failed to send message: {ex.Message}", Severity.Error);
                    }
                }
            }

            _loading = false;
        }

        private async Task LogoutPlayer(OnlinePlayer? player)
        {
            _loading = true;
            if (player == null)
            {
                _loading = false;
                return;
            }

            if (player.Location is not EFikaLocation.None and not EFikaLocation.Hideout)
            {
                Snackbar.Add($"{player.Nickname} is in a raid and cannot be logged out", Severity.Warning);
                _loading = false;
                return;
            }

            ProfileIdRequest request = new()
            {
                ProfileId = player.ProfileId
            };

            try
            {
                var result = await HttpClient.PostAsJsonAsync("post/logout", request);
                Snackbar.Add($"Sent logout message to {player.Nickname}", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Failed to send logout request: {ex.Message}", Severity.Error);
            }

            _loading = false;
        }

#if DEBUG
        protected override void OnInitialized()
        {
            base.OnInitialized();

            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.Labyrinth,
                Nickname = "John",
                ProfileId = "test"
            });
            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.Customs,
                Nickname = "West",
                ProfileId = "test"
            });
            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.Streets,
                Nickname = "Bjorn",
                ProfileId = "test"
            });
            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.Hideout,
                Nickname = "Roland",
                ProfileId = "test"
            });
            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.None,
                Nickname = "TarkovMan1337",
                ProfileId = "test"
            });
            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.Woods,
                Nickname = "Janky",
                ProfileId = "test"
            });
            _players.Add(new()
            {
                Level = Random.Shared.Next(1, 69),
                Location = FikaShared.Enums.EFikaLocation.GroundZero,
                Nickname = "guidot",
                ProfileId = "test"
            });
        }
#endif
    }
}