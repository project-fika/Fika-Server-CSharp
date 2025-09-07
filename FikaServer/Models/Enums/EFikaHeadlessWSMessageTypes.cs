namespace FikaServer.Models.Enums;

public enum EFikaHeadlessWSMessageType
{
    KeepAlive = 0,
    HeadlessStartRaid = 1,
    RequesterJoinMatch = 2,
    ShutdownClient = 4
}
