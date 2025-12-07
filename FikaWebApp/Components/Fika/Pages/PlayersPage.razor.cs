using System.Net;
using FikaShared;
using FikaShared.Requests;
#if RELEASE
using FikaShared.Responses; 
#endif
using FikaWebApp.Components.Fika.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static System.Net.WebRequestMethods;
using static FikaShared.Enums;

namespace FikaWebApp.Components.Fika.Pages;

public partial class PlayersPage
{
    [Inject]
    ILogger<ProfilesPage> Logger { get; set; } = default!;

    private List<OnlinePlayer> _players = [];
    private bool _loading;

#if RELEASE
	protected override async Task OnInitializedAsync()
	{
		_loading = true;
		await base.OnInitializedAsync();

		try
		{
			var result = await HttpClient.GetFromJsonAsync<GetOnlinePlayersResponse>("fika/api/players");
            if (result?.Players.Count > 0)
			{
				_players = result.Players;
			}
            else
            {
                Snackbar.Add("No players online", Severity.Info);
            }
		}
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.Forbidden)
            {
                Snackbar.Add("Something went wrong when querying for heartbeat: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                Logger.LogError("Something went wrong when querying for heartbeat: [403 Forbidden]. Are you using the wrong API key?");
            }
            else if (httpEx.StatusCode is HttpStatusCode.NotFound)
            {
                Snackbar.Add("Something went wrong when querying for heartbeat: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                Logger.LogError("Something went wrong when querying for heartbeat: [404 NotFound]. Are you missing the Fika server mod?");
            }
            else
            {
                Snackbar.Add($"There was a HttpRequestException caught when when querying for heartbeat:\n{httpEx.Message}", Severity.Error);
                Logger.LogError("There was a HttpRequestException caught when when querying for heartbeat: {HttpException}", httpEx.Message);
            }
        }
        catch (Exception ex)
		{
			Snackbar.Add($"There was an error retrieving the players: {ex.Message}", Severity.Error);
            Logger.LogError("There was a HttpRequestException caught when when querying for heartbeat: {HttpException}", ex.Message);
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
                    var postResult = await HttpClient.PostAsJsonAsync("fika/api/sendmessage", request);
                    Snackbar.Add($"Message sent to {player.Nickname}", Severity.Success);
                }
                catch (HttpRequestException httpEx)
                {
                    if (httpEx.StatusCode is HttpStatusCode.Forbidden)
                    {
                        Snackbar.Add("Something went wrong when sending the message: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                        Logger.LogError("Something went wrong when sending the message: [403 Forbidden]. Are you using the wrong API key?");
                    }
                    else if (httpEx.StatusCode is HttpStatusCode.NotFound)
                    {
                        Snackbar.Add("Something went wrong when sending the message: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                        Logger.LogError("Something went wrong when sending the message: [404 NotFound]. Are you missing the Fika server mod?");
                    }
                    else
                    {
                        Snackbar.Add($"There was a HttpRequestException caught when when sending the message:\n{httpEx.Message}", Severity.Error);
                        Logger.LogError("There was a HttpRequestException caught when when sending the message: {HttpException}", httpEx.Message);
                    }
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
            var result = await HttpClient.PostAsJsonAsync("fika/api/logout", request);
            Snackbar.Add($"Sent logout message to {player.Nickname}", Severity.Success);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.Forbidden)
            {
                Snackbar.Add("Something went wrong when sending the logout request: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                Logger.LogError("Something went wrong when sending the logout request: [403 Forbidden]. Are you using the wrong API key?");
            }
            else if (httpEx.StatusCode is HttpStatusCode.NotFound)
            {
                Snackbar.Add("Something went wrong when sending the logout request: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                Logger.LogError("Something went wrong when sending the logout request: [404 NotFound]. Are you missing the Fika server mod?");
            }
            else
            {
                Snackbar.Add($"There was a HttpRequestException caught when when sending the logout request:\n{httpEx.Message}", Severity.Error);
                Logger.LogError("There was a HttpRequestException caught when when sending the logout request: {HttpException}", httpEx.Message);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Failed to send logout request: {ex.Message}", Severity.Error);
        }

        _loading = false;
    }

#if DEBUG
    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        await base.OnInitializedAsync();

        await Task.Delay(TimeSpan.FromSeconds(1));

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

        _loading = false;
    }
#endif
}