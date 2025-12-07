using System.Net;
using FikaShared.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages;

public partial class StatisticsPage
{
    [Inject]
    private HttpClient HttpClient { get; set; } = default!;

    [Inject]
    private ILogger<StatisticsPage> Logger { get; set; } = default!;
    
    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private readonly List<StatisticsPlayer> _players = [];
    private bool _loading;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

#if DEBUG
        _loading = true;
        _players.Add(new StatisticsPlayer
        {
            AmmoUsed = Random.Shared.NextDouble() * 100_000,
            ArmorDamage = Random.Shared.NextDouble() * 100_000,
            BodyDamage = Random.Shared.NextDouble() * 100_000,
            BossKills = Random.Shared.NextDouble() * 100,
            Deaths = Random.Shared.NextDouble() * 1000,
            Headshots = Random.Shared.NextDouble() * 100,
            Kills = Random.Shared.NextDouble() * 10_000,
            Nickname = "Test1"
        });
        _players.Add(new StatisticsPlayer
        {
            AmmoUsed = Random.Shared.NextDouble() * 100_000,
            ArmorDamage = Random.Shared.NextDouble() * 100_000,
            BodyDamage = Random.Shared.NextDouble() * 100_000,
            BossKills = Random.Shared.NextDouble() * 100,
            Deaths = Random.Shared.NextDouble() * 1000,
            Headshots = Random.Shared.NextDouble() * 100,
            Kills = Random.Shared.NextDouble() * 10_000,
            Nickname = "Test2"
        });
        _players.Add(new StatisticsPlayer
        {
            AmmoUsed = Random.Shared.NextDouble() * 100_000,
            ArmorDamage = Random.Shared.NextDouble() * 100_000,
            BodyDamage = Random.Shared.NextDouble() * 100_000,
            BossKills = Random.Shared.NextDouble() * 100,
            Deaths = Random.Shared.NextDouble() * 1000,
            Headshots = Random.Shared.NextDouble() * 100,
            Kills = Random.Shared.NextDouble() * 10_000,
            Nickname = "Test3"
        });
        _players.Add(new StatisticsPlayer
        {
            AmmoUsed = Random.Shared.NextDouble() * 100_000,
            ArmorDamage = Random.Shared.NextDouble() * 100_000,
            BodyDamage = Random.Shared.NextDouble() * 100_000,
            BossKills = Random.Shared.NextDouble() * 100,
            Deaths = Random.Shared.NextDouble() * 1000,
            Headshots = Random.Shared.NextDouble() * 100,
            Kills = Random.Shared.NextDouble() * 10_000,
            Nickname = "Test4"
        });
        _players.Add(new StatisticsPlayer
        {
            AmmoUsed = Random.Shared.NextDouble() * 100_000,
            ArmorDamage = Random.Shared.NextDouble() * 100_000,
            BodyDamage = Random.Shared.NextDouble() * 100_000,
            BossKills = Random.Shared.NextDouble() * 100,
            Deaths = Random.Shared.NextDouble() * 1000,
            Headshots = Random.Shared.NextDouble() * 100,
            Kills = Random.Shared.NextDouble() * 10_000,
            Nickname = "Test5"
        });
        await Task.Delay(TimeSpan.FromSeconds(1)); // simulate loading
        _loading = false;
#else
        try
        {
            _loading = true;
            var result = await HttpClient.GetFromJsonAsync<GetStatisticsResponse>("fika/api/statistics");
            _players.AddRange(result!.Players);
        }
        catch (HttpRequestException httpEx)
        {
            if (httpEx.StatusCode is HttpStatusCode.Forbidden)
            {
                Snackbar.Add("Something went wrong when retrieving statistics: [403 Forbidden].\nAre you using the wrong API key?", Severity.Error);
                Logger.LogError("Something went wrong when retrieving statistics: [403 Forbidden]. Are you using the wrong API key?");
            }
            else if (httpEx.StatusCode is HttpStatusCode.NotFound)
            {
                Snackbar.Add("Something went wrong when retrieving statistics: [404 NotFound].\nAre you missing the Fika server mod?", Severity.Error);
                Logger.LogError("Something went wrong when retrieving statistics: [404 NotFound]. re you missing the Fika server mod?");
            }
            else
            {
                Snackbar.Add($"There was a HttpRequestException caught when when retrieving statistics:\n{httpEx.Message}", Severity.Error);
                Logger.LogError("There was a HttpRequestException caught when when retrieving statistics: {HttpException}", httpEx.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("There was an error retrieving the statistics: {Exception}", ex.Message);
            Snackbar.Add($"There was an error retrieving the statistics: {ex.Message}", Severity.Error);
        }

        _loading = false;
#endif

    }
}