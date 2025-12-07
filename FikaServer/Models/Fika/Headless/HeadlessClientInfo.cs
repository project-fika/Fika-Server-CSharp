using FikaServer.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FikaServer.Models.Fika.Headless;

/// <summary>
/// Record containing data for a headless client
/// </summary>
public record HeadlessClientInfo
{
    [SetsRequiredMembers]
    public HeadlessClientInfo(System.Net.WebSockets.WebSocket webSocket, EHeadlessStatus state)
    {
        WebSocket = webSocket;
        State = state;
    }

    /// <summary>
    /// Websocket of the headless client
    /// </summary>
    public required System.Net.WebSockets.WebSocket WebSocket { get; set; }
    /// <summary>
    /// State of the headless client
    /// </summary>
    public required EHeadlessStatus State { get; set; }
    /// <summary>
    /// The players that are playing on this headless client, only set if the state is IN_RAID
    /// </summary>
    public List<string>? Players { get; set; }
    /// <summary>
    /// SessionID of the person who has requested the headless client, it will only ever be set if the status is IN_RAID
    /// </summary>
    public string? RequesterSessionID { get; set; }
    /// <summary>
    /// Allows for checking if the requester has been notified the match has started through the requester WebSocket so he can auto-join
    /// </summary>
    public bool? HasNotifiedRequester { get; set; }
    /// <summary>
    /// The current level of the headless, mainly used for transits
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Sets the info of the client after starting a raid
    /// </summary>
    /// <param name="requestId">The id who requested the raid</param>
    public void StartRaid(string requestId)
    {
        Players = [];
        RequesterSessionID = requestId;
        HasNotifiedRequester = false;
    }

    /// <summary>
    /// Resets the data of the <see cref="HeadlessClientInfo"/>
    /// </summary>
    public void Reset()
    {
        State = EHeadlessStatus.READY;
        Players = null;
        RequesterSessionID = null;
        HasNotifiedRequester = null;
    }
}
