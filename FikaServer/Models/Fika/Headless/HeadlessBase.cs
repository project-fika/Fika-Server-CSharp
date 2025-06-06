using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.Headless
{
    public abstract record HeadlessBase
    {
        public required EFikaHeadlessWSMessageType Type { get; set; }
    }
}
