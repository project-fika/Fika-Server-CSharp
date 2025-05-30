using FikaServer.Helpers;
using FikaServer.Models.Fika;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.Models.Fika.Routes.Raid;
using FikaServer.Models.Fika.Routes.Raid.Create;
using FikaServer.Models.Fika.Routes.Raid.Host;
using FikaServer.Models.Fika.Routes.Raid.Join;
using FikaServer.Models.Fika.Routes.Raid.Leave;
using FikaServer.Models.Fika.Routes.Raid.Settings;
using FikaServer.Models.Fika.WebSocket.Notifications;
using FikaServer.Services;
using FikaServer.Services.Headless;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers.Ws;

namespace FikaServer.Controllers
{
    [Injectable]
    public class RaidController(MatchService matchService, HeadlessHelper headlessHelper, HeadlessService headlessService, ISptLogger<RaidController> logger, InRaidController inraidController, IEnumerable<IWebSocketConnectionHandler> sptWebSocketConnectionHandlers)
    {
        /// <summary>
        /// Handle /fika/raid/create
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FikaRaidCreateResponse HandleRaidCreate(FikaRaidCreateRequestData request, string sessionId)
        {
            string hostUsername = request.HostUsername;

            if (headlessHelper.IsHeadlessClient(request.ServerId))
            {
                hostUsername = headlessHelper.GetHeadlessNickname(request.ServerId);
            }

            var NotificationWebSocket = sptWebSocketConnectionHandlers
                .OfType<NotificationWebSocket>()
                .FirstOrDefault(wsh => wsh.GetSocketId() == "Fika Notification Manager");

            _ = NotificationWebSocket!.BroadcastAsync(new StartRaidNotification
            {
                Nickname = hostUsername,
                Location = request.Settings.Location,
                IsHeadlessRaid = headlessHelper.IsHeadlessClient(request.ServerId),
                HeadlessRequesterName = headlessHelper.GetRequesterUsername(request.ServerId) ?? "",
                RaidTime = request.Time
            });

            return new FikaRaidCreateResponse
            {
                Success = matchService.CreateMatch(request, sessionId)
            };
        }

        /// <summary>
        /// Handle /fika/raid/join
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FikaRaidJoinResponse HandleRaidJoin(FikaRaidJoinRequestData request)
        {
            FikaMatch match = matchService.GetMatch(request.ServerId);

            return new FikaRaidJoinResponse
            {
                ServerId = request.ServerId,
                Timestamp = match.Timestamp,
                GameVersion = match.GameVersion,
                CRC32 = match.CRC32,
                RaidCode = match.RaidCode,
            };
        }

        /// <summary>
        /// Handle /fika/raid/leave
        /// </summary>
        /// <param name="request"></param>
        public void HandleRaidLeave(FikaRaidLeaveRequestData request)
        {
            if (request.ServerId == request.ProfileId)
            {
                matchService.EndMatch(request.ServerId, Models.Enums.EFikaMatchEndSessionMessage.HostShutdown);
                return;
            }

            matchService.RemovePlayerFromMatch(request.ServerId, request.ProfileId);
        }

        /// <summary>
        /// Handle /fika/raid/gethost
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FikaRaidGethostResponse? HandleRaidGetHost(FikaRaidServerIdRequestData request)
        {
            var match = matchService.GetMatch(request.ServerId);

            if (match == null)
            {
                return null;
            }

            return new FikaRaidGethostResponse
            {
                Ips = match.Ips,
                Port = match.Port,
                NatPunch = match.NatPunch,
                IsHeadless = match.IsHeadless,
            };
        }

        /// <summary>
        /// Handle /fika/raid/getsettings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public FikaRaidSettingsResponse? HandleRaidGetSettings(FikaRaidServerIdRequestData request)
        {
            var match = matchService.GetMatch(request.ServerId);

            if (match == null)
            {
                return null;
            }

            return new FikaRaidSettingsResponse
            {
                MetabolismDisabled = match.RaidConfig.MetabolismDisabled,
                PlayersSpawnPlace = match.RaidConfig.PlayersSpawnPlace,
                HourOfDay = match.RaidConfig.TimeAndWeatherSettings.HourOfDay,
                TimeFlowType = match.RaidConfig.TimeAndWeatherSettings.TimeFlowType
            };
        }

        /// <summary>
        /// Handle /fika/raid/headless/start
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public StartHeadlessResponse HandleRaidStartHeadless(string sessionID, StartHeadlessRequest info)
        {
            if (!headlessHelper.IsHeadlessClientAvailable(info.HeadlessSessionID))
            {
                return new StartHeadlessResponse
                {
                    MatchId = null,
                    Error = "This headless client is not available."
                };
            }

            if (headlessHelper.IsHeadlessClient(sessionID))
            {
                return new StartHeadlessResponse
                {
                    MatchId = null,
                    Error = "You are trying to connect to a headless client while having Fika.Headless installed. Please remove Fika.Headless from your client and try again."
                };
            }

            // TODO: Fix async
            string? headlessClientId = headlessService.StartHeadlessRaid(info.HeadlessSessionID, sessionID, info);

            logger.Info($"Sent WS FikaHeadlessStartRaid to {headlessClientId}");

            return new StartHeadlessResponse
            {
                // This really isn't required, I just want to make sure on the client
                MatchId = headlessClientId,
                Error = null
            };
        }

        /// <summary>
        /// Handle /fika/raid/registerPlayer
        /// </summary>
        /// <param name="SessionID"></param>
        /// <param name="info"></param>
        public void HandleRaidRegisterPlayer(string SessionID, RegisterPlayerRequestData info)
        {
            inraidController.AddPlayer(SessionID, info);
        }
    }
}
