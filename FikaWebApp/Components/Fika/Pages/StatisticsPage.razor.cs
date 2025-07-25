using FikaShared.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace FikaWebApp.Components.Fika.Pages
{
    public partial class StatisticsPage
    {
        [Inject]
        private HttpClient HttpClient { get; set; } = default!;

        [Inject]
        private ILogger<StatisticsPage> Logger { get; set; } = default!;

        private readonly List<StatisticsPlayer> _players = [];

        private string[]? _nicknames;
        private double[]? _kills;
        private double[]? _deaths;
        private double[]? _ammoUsed;
        private double[]? _bodyDamage;
        private double[]? _armorDamage;
        private double[]? _headshots;
        private double[]? _bossKills;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            try
            {
                var result = await HttpClient.GetFromJsonAsync<GetStatisticsResponse>("get/statistics");
                _players.AddRange(result!.Players);

                _nicknames = [.. _players.Select(p => p.Nickname)];
                _kills = [.. _players.Select(p => p.Kills)];
                _deaths = [.. _players.Select(p => p.Deaths)];
                _ammoUsed = [.. _players.Select(p => p.AmmoUsed)];
                _bodyDamage = [.. _players.Select(p => p.BodyDamage)];
                _armorDamage = [.. _players.Select(p => p.ArmorDamage)];
                _headshots = [.. _players.Select(p => p.Headshots)];
                _bossKills = [.. _players.Select(p => p.BossKills)];
            }
            catch (Exception ex)
            {
                Logger.LogError("There was an error retrieving the statistics: {Exception}", ex.Message);
            }
        }
    }
}