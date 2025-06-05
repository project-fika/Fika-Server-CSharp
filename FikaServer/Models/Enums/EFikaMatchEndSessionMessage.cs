using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.Models.Enums
{
    [EftEnumConverter]
    public enum EFikaMatchEndSessionMessage
    {
        HostShutdown = 0,
        PingTimeout = 1,
        NoPlayers = 2,
    }
}
