using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.Headless
{
    public abstract record HeadlessBase
    {
        public required abstract EFikaHeadlessWSMessageType Type { get; set; }
    }
}
