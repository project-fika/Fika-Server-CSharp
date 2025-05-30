using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.Models.Enums
{
    [EftEnumConverter]
    public enum EFikaMatchStatus
    {
        LOADING = 0,
        IN_GAME = 1,
        COMPLETE = 2,
    }
}
