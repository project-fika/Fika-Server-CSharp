using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Routes.Headless;
using System.Diagnostics.CodeAnalysis;

namespace FikaServer.Models.Fika.Headless
{
    public record StartHeadlessRaid : IHeadlessWSMessage
    {
        [SetsRequiredMembers]
        public StartHeadlessRaid(StartHeadlessRequest request)
        {
            StartHeadlessRequest = request;
        }

        public EFikaHeadlessWSMessageType Type {  get; set; } = EFikaHeadlessWSMessageType.HeadlessStartRaid;
        public required StartHeadlessRequest? StartHeadlessRequest { get; set; }
    }
}
