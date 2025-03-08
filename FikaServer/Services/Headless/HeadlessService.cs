using FikaServer.Models.Fika.Headless;
using FikaServer.Models.Fika.Routes.Headless;
using SPTarkov.Common.Annotations;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace FikaServer.Services.Headless
{
    [Injectable(InjectionType.Singleton)]
    public class HeadlessService
    {
        public ConcurrentDictionary<string, HeadlessClientInfo> GetHeadlessClients()
        {
            //Todo: Stub for now, implement method.
            return [];
        }

        public void AddHeadlessClient(string sessionID, WebSocket webSocket)
        {
            //Todo: Stub for now, implement method.
        }

        public void RemoveHeadlessClient(string sessionID)
        {
            //Todo: Stub for now, implement method.
        }

        public string? StartHeadlessRaid(string headlessSessionID, string requesterSessionID, StartHeadlessRequest info)
        {
            //Todo: Stub for now, implement method.
            return string.Empty;
        }

        public void SendJoinMessageToRequester(string headlessClientId)
        {
            //Todo: Stub for now, implement method.
        }

        public void AddPlayerToHeadlessMatch(string headlessClientId, string sessionID)
        {
            //Todo: Stub for now, implement method.
        }

        public void SetHeadlessLevel(string headlessClientId)
        {
            //Todo: Stub for now, implement method.
        }

        public void EndHeadlessRaid(string headlessClientId)
        {
            //Todo: Stub for now, implement method.
        }
    }
}
