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
        catch (Exception ex)
        {
            Logger.LogError("There was an error retrieving the statistics: {Exception}", ex.Message);
            Snackbar.Add($"There was an error retrieving the statistics: {ex.Message}", Severity.Error);
        }

        _loading = false;
#endif

    }
}