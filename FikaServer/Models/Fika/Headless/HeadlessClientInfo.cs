using FikaServer.Models.Enums;
using System.Net.WebSockets;

namespace FikaServer.Models.Fika.Headless
{
    public record HeadlessClientInfo
    {
        /** Websocket of the headless client */
        public required WebSocket WebSocket { get;set; }
        /** State of the headless client */
        public required EHeadlessStatus State { get; set; }
        /** The players that are playing on this headless client, only set if the state is IN_RAID */
        public List<string>? Players { get; set; }
        /** SessionID of the person who has requested the headless client, it will only ever be set if the status is IN_RAID */
        public string? RequesterSessionID { get; set; }
        /** Allows for checking if the requester has been notified the match has started through the requester WebSocket so he can auto-join */
        public string? HasNotifiedRequester { get; set; }
    }
}
