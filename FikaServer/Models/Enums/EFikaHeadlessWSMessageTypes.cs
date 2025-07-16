using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.Models.Enums
{
    [EftEnumConverter]
    public enum EFikaHeadlessWSMessageType
    {
        KeepAlive = 0,
        HeadlessStartRaid = 1,
        RequesterJoinMatch = 2,
        ShutdownClient = 4
    }
}
