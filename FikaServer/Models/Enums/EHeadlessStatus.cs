using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.Models.Enums
{
    [EftEnumConverter]
    public enum EHeadlessStatus
    {
        READY = 1,
        IN_RAID = 2,
    }
}
