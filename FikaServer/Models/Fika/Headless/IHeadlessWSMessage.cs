using FikaServer.Models.Enums;

namespace FikaServer.Models.Fika.Headless
{
    public interface IHeadlessWSMessage
    {
        public abstract EFikaHeadlessWSMessageType Type { get; set; }
    }
}
