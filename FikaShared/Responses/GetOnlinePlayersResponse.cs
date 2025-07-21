namespace FikaShared.Responses
{
    public record GetOnlinePlayersResponse
    {
        public OnlinePlayer Players { get; set; }
    }
}
