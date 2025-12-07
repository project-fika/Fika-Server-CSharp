using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.Routes.Headless;
using FikaServer.WebSockets;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace FikaServer.Services.Headless;

[Injectable(InjectionType.Singleton)]
public class HeadlessService(ISptLogger<HeadlessService> logger,
    HeadlessRequesterWebSocket headlessRequesterWebSocket, JsonUtil jsonUtil,
    ConfigService fikaConfigService, SaveServer saveServer)
{
    public ConcurrentDictionary<MongoId, HeadlessClientInfo> HeadlessClients { get; } = [];

    /// <summary>
    /// Begin setting up a raid for a headless client
    /// </summary>
    /// <returns>returns the SessionID of the headless client that is starting this raid, returns null if no client could be found or there was an error.</returns>
    public async Task<string?> StartHeadlessRaid(string headlessSessionID, string requesterSessionID, StartHeadlessRequest info)
    {
        if (!HeadlessClients.TryGetValue(headlessSessionID, out var headlessClientInfo))
        {
            logger.LogWithColor($"Could not find HeadlessSessionID '{headlessSessionID}'", LogTextColor.Red);
            return null;
        }

        if (headlessClientInfo.State is not EHeadlessStatus.READY)
        {
            logger.LogWithColor($"HeadlessSessionID '{headlessSessionID}' was not ready, was {headlessClientInfo.State}", LogTextColor.Yellow);
            return null;
        }

        var webSocket = headlessClientInfo.WebSocket;
        if (webSocket == null)
        {
            return null;
        }

        if (webSocket.State is WebSocketState.Closed)
        {
            return null;
        }

        headlessClientInfo.StartRaid(requesterSessionID);

        StartHeadlessRaid startHeadlessRequest = new(info);
        var data = jsonUtil.Serialize(startHeadlessRequest)
            ?? throw new NullReferenceException("StartHeadlessRaid:: Data was null after serializing");
        await webSocket.SendAsync(Encoding.UTF8.GetBytes(data),
            WebSocketMessageType.Text, true, CancellationToken.None);

        return headlessSessionID;
    }

    /// <summary>
    /// Sends a join message to the requester of a headless client
    /// </summary>
    public async Task SendJoinMessageToRequester(string headlessClientId)
    {
        if (!HeadlessClients.TryGetValue(headlessClientId, out var headlessClientInfo))
        {
            logger.LogWithColor($"Could not find HeadlessSessionID '{headlessClientId}'", LogTextColor.Red);
            return;
        }

        if (headlessClientInfo.State is not EHeadlessStatus.READY)
        {
            logger.LogWithColor($"HeadlessSessionID '{headlessClientId}' was not ready, was {headlessClientInfo.State}", LogTextColor.Yellow);
            return;
        }

        headlessClientInfo.State = EHeadlessStatus.IN_RAID;

        await headlessRequesterWebSocket.SendAsync(headlessClientInfo.RequesterSessionID,
            new HeadlessRequesterJoinRaid(headlessClientId));
    }

    public void AddPlayerToHeadlessMatch(MongoId headlessClientId, MongoId sessionID)
    {
        if (HeadlessClients.TryGetValue(headlessClientId, out var headlessClientInfo))
        {
            if (headlessClientInfo == null)
            {
                throw new NullReferenceException($"AddPlayerToHeadlessMatch:: HeadlessClientInfo was null on {headlessClientId}");
            }

            headlessClientInfo.Players?.Add(sessionID);

            if (!fikaConfigService.Config.Headless.SetLevelToAverageOfLobby)
            {
                // Doing this everytime is unecessary if we're not setting the average level so only set it once the original requester of the headless joins.
                if (headlessClientInfo.RequesterSessionID == sessionID)
                {
                    SetHeadlessLevel(headlessClientId);
                }
            }
            else
            {
                SetHeadlessLevel(headlessClientId);
            }
        }
    }

    public void SetHeadlessLevel(MongoId headlessClientId)
    {
        if (!HeadlessClients.TryGetValue(headlessClientId, out var headlessClientInfo))
        {
            throw new NullReferenceException($"SetHeadlessLevel:: Could not find headlessClientId '{headlessClientId}'");
        }

        if (headlessClientInfo.State is not EHeadlessStatus.IN_RAID)
        {
            return;
        }

        var headlessProfile = saveServer.GetProfile(headlessClientId)
            ?? throw new NullReferenceException($"Could not find headlessProfile {headlessClientId}");

        var baseHeadlessLevel = 1;
        var countedPlayers = 0; // count only valid, non-headless players

        foreach (var profileId in headlessClientInfo.Players)
        {
            var profile = saveServer.GetProfile(profileId);
            if (profile?.IsHeadlessProfile() != false)
            {
                continue;
            }

            baseHeadlessLevel += profile.CharacterData.PmcData.Info.Level ?? 1;
            countedPlayers++;
        }

        // avoid division by zero
        if (countedPlayers > 0)
        {
            baseHeadlessLevel = Math.Max(1, baseHeadlessLevel / countedPlayers);
        }
        else
        {
            baseHeadlessLevel = 1;
        }

        logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Debug,
            $"[{headlessClientId}] Settings headless level to: {baseHeadlessLevel} | Players: {countedPlayers}");

        headlessProfile.CharacterData.PmcData.Info.Level = baseHeadlessLevel;
    }

    /// <summary>
    /// End the raid for the specified headless client, sets the state back to READY so that he can be requested to host again.
    /// </summary>
    public void EndHeadlessRaid(string headlessClientId)
    {
        if (!HeadlessClients.TryGetValue(headlessClientId, out var headlessClientInfo))
        {
            logger.LogWithColor($"EndHeadlessRaid:: Could not find '{headlessClientId}' to remove");
            return;
        }

        headlessClientInfo.Reset();
    }
}
