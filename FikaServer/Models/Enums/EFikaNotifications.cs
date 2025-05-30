using SPTarkov.Server.Core.Utils.Json.Converters;

namespace FikaServer.Models.Enums
{
    [EftEnumConverter]
    public enum EFikaNotifications
    {
        KeepAlive = 0,
        StartedRaid = 1,
        SentItem = 2,
        PushNotification = 3,
    }
}
