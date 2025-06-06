using FikaServer.Models.Enums;
using System.Diagnostics.CodeAnalysis;

namespace FikaServer.Models.Fika.Headless
{
    public record HeadlessRequesterJoinRaid : HeadlessBase
    {
        [SetsRequiredMembers]
        public HeadlessRequesterJoinRaid(EFikaHeadlessWSMessageType type, string matchId)
        {
            Type = type;
            MatchId = matchId;
        }

        public required string? MatchId { get; set; }
    }
}
