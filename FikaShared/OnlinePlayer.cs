using static FikaShared.Enums;

namespace FikaShared.Responses
{
    public record OnlinePlayer
    {
        public required string ProfileId { get; set; }
        public required string Nickname { get; set; }
        public required int Level { get; set; }
        public required EFikaLocation Location { get; set; }
    }
}