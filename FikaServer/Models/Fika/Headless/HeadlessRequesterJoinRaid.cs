using FikaServer.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FikaServer.Models.Fika.Headless;

public record HeadlessRequesterJoinRaid : IHeadlessWSMessage
{
    [SetsRequiredMembers]
    public HeadlessRequesterJoinRaid(string matchId)
    {
        if (string.IsNullOrEmpty(matchId))
        {
            throw new NullReferenceException("matchId was missing");
        }

        MatchId = matchId;
    }

    public EFikaHeadlessWSMessageType Type { get; set; } = EFikaHeadlessWSMessageType.RequesterJoinMatch;
    public required string? MatchId { get; set; }
}
