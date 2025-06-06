using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Routes.Headless;
using System.Diagnostics.CodeAnalysis;

namespace FikaServer.Models.Fika.Headless
{
    public record StartHeadlessRaid : HeadlessBase
    {
        [SetsRequiredMembers]
        public StartHeadlessRaid(EFikaHeadlessWSMessageType type, StartHeadlessRequest request)
        {
            Type = type;
            StartHeadlessRequest = request;
        }

        public required StartHeadlessRequest? StartHeadlessRequest { get; set; }
    }
}
