
using FikaServer.Services;
using FikaServer.Services.Cache;
using FikaShared;
using FikaShared.Responses;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Text;
using static FikaShared.Enums;

namespace FikaServer.Http.Get
{
    [Injectable(TypePriority = 0)]
    public class HttpGetPlayers(ConfigService configService, FikaProfileService profileService,
        SaveServer saveServer, PresenceService presenceService, HttpResponseUtil httpResponseUtil) : BaseHttpRequest(configService)
    {
        public override string Path { get; set; } = "/get/players";

        public override string Method
        {
            get
            {
                return HttpMethods.Get;
            }
        }

        public override async Task HandleRequest(HttpRequest req, HttpResponse resp)
        {
            var players = profileService.GetAllProfiles();
            var profiles = new List<SptProfile>(players.Count);
            foreach (var profileId in players.Values)
            {
                var profile = saveServer.GetProfile(profileId);
                if (profile != null)
                {
                    profiles.Add(profile);
                }
            }

            var presences = presenceService.AllPlayersPresence;
            var onlinePlayers = new List<OnlinePlayer>(presences.Count);
            profiles = [.. profiles.Where(x => x.HasProfileData() && presences.Any(p => p.Nickname == x.CharacterData.PmcData.Info.Nickname))];
            foreach (var profile in profiles)
            {
                var profileId = profile.ProfileInfo.ProfileId.Value;
                var presence = presenceService.GetPlayerPresence(profileId);

                EFikaLocation location;
                if (presence != null)
                {
                    location = presence.ToFikaLocation();
                }
                else
                {
                    location = EFikaLocation.None;
                }

                onlinePlayers.Add(new()
                {
                    ProfileId = profileId,
                    Nickname = profile.CharacterData?.PmcData?.Info?.Nickname ?? "Unknown",
                    Level = profile.CharacterData?.PmcData?.Info?.Level ?? 1,
                    Location = location
                });
            }

            GetOnlinePlayersResponse playersResponse = new()
            {
                Players = onlinePlayers
            };

            resp.StatusCode = 200;
            resp.ContentType = ContentTypes.Json;
            await resp.Body.WriteAsync(Encoding.UTF8.GetBytes(httpResponseUtil.NoBody(playersResponse)));
            await resp.StartAsync();
            await resp.CompleteAsync();
        }
    }
}
