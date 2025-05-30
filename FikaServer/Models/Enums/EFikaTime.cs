using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.Models.Enums
{
    [EftEnumConverter]
    public enum EFikaTime
    {
        CURR = 0,
        PAST = 1,
    }
}
