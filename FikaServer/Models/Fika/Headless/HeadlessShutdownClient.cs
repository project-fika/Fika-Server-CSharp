using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.Headless
{
    public record HeadlessShutdownClient : IHeadlessWSMessage
    {
        public EFikaHeadlessWSMessageType Type { get; set; } = EFikaHeadlessWSMessageType.ShutdownClient;
    }
}
